using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Notification.Infrastructure.Persistence.Inbox;

public sealed class NotificationInboxStore
    : INotificationInboxStore
{
    private const string ClaimSql =
        """
        WITH candidates AS
        (
            SELECT "Id"
            FROM "InboxMessages"
            WHERE
                (
                    "Status" = @pendingStatus
                    AND "NextAttemptAtUtc" <= @nowUtc
                )
                OR
                (
                    "Status" = @processingStatus
                    AND "LockedUntilUtc" <= @nowUtc
                )
            ORDER BY "ReceivedOnUtc", "Id"
            LIMIT @batchSize
            FOR UPDATE SKIP LOCKED
        )
        UPDATE "InboxMessages" AS inbox
        SET
            "Status" = @processingStatus,
            "AttemptCount" = inbox."AttemptCount" + 1,
            "LockId" = @lockId,
            "LockedUntilUtc" = @lockedUntilUtc
        FROM candidates
        WHERE inbox."Id" = candidates."Id"
        RETURNING
            inbox."Id",
            inbox."EventType",
            inbox."RoutingKey",
            inbox."Payload",
            inbox."OccurredOnUtc",
            inbox."AttemptCount",
            inbox."LockId";
        """;

    private readonly NotificationDbContext _dbContext;

    public NotificationInboxStore(
        NotificationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ClaimedInboxMessage>> ClaimAsync(
        Guid lockId,
        DateTime nowUtc,
        DateTime lockedUntilUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        ValidateClaimArguments(
            lockId,
            nowUtc,
            lockedUntilUtc,
            batchSize);

        await _dbContext.Database.OpenConnectionAsync(
            cancellationToken);

        try
        {
            var connection =
                (NpgsqlConnection)_dbContext.Database
                    .GetDbConnection();

            await using var command =
                new NpgsqlCommand(
                    ClaimSql,
                    connection);

            command.Parameters.AddWithValue(
                "pendingStatus",
                (int)InboxMessageStatus.Pending);

            command.Parameters.AddWithValue(
                "processingStatus",
                (int)InboxMessageStatus.Processing);

            command.Parameters.AddWithValue(
                "nowUtc",
                nowUtc);

            command.Parameters.AddWithValue(
                "lockedUntilUtc",
                lockedUntilUtc);

            command.Parameters.AddWithValue(
                "batchSize",
                batchSize);

            command.Parameters.AddWithValue(
                "lockId",
                lockId);

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var messages =
                new List<ClaimedInboxMessage>();

            int idOrdinal =
                reader.GetOrdinal("Id");

            int eventTypeOrdinal =
                reader.GetOrdinal("EventType");

            int routingKeyOrdinal =
                reader.GetOrdinal("RoutingKey");

            int payloadOrdinal =
                reader.GetOrdinal("Payload");

            int occurredOnUtcOrdinal =
                reader.GetOrdinal("OccurredOnUtc");

            int attemptCountOrdinal =
                reader.GetOrdinal("AttemptCount");

            int lockIdOrdinal =
                reader.GetOrdinal("LockId");

            while (await reader.ReadAsync(cancellationToken))
            {
                messages.Add(
                    new ClaimedInboxMessage(
                        reader.GetGuid(idOrdinal),
                        reader.GetGuid(lockIdOrdinal),
                        reader.GetString(eventTypeOrdinal),
                        reader.GetString(routingKeyOrdinal),
                        reader.GetString(payloadOrdinal),
                        reader.GetDateTime(
                            occurredOnUtcOrdinal),
                        reader.GetInt32(
                            attemptCountOrdinal)));
            }

            return messages;
        }
        finally
        {
            await _dbContext.Database
                .CloseConnectionAsync();
        }
    }

    public async Task<bool> MarkProcessedAsync(
        Guid messageId,
        Guid lockId,
        DateTime processedOnUtc,
        CancellationToken cancellationToken = default)
    {
        ValidateOwnershipArguments(
            messageId,
            lockId);

        EnsureUtc(
            processedOnUtc,
            nameof(processedOnUtc),
            "Inbox processed time must be UTC.");

        int affectedRows =
            await _dbContext.Database
                .ExecuteSqlInterpolatedAsync(
                    $"""
                    UPDATE "InboxMessages"
                    SET
                        "Status" = {(int)InboxMessageStatus.Processed},
                        "ProcessedOnUtc" = {processedOnUtc},
                        "LockId" = NULL,
                        "LockedUntilUtc" = NULL,
                        "LastError" = NULL
                    WHERE "Id" = {messageId}
                    AND "Status" = {(int)InboxMessageStatus.Processing}
                    AND "LockId" = {lockId};
                    """,
                    cancellationToken);

        return affectedRows == 1;
    }

    public async Task<bool> ScheduleRetryAsync(
        Guid messageId,
        Guid lockId,
        DateTime nextAttemptAtUtc,
        string error,
        CancellationToken cancellationToken = default)
    {
        ValidateOwnershipArguments(
            messageId,
            lockId);

        EnsureUtc(
            nextAttemptAtUtc,
            nameof(nextAttemptAtUtc),
            "Inbox next attempt time must be UTC.");

        string normalizedError =
            NormalizeError(error);

        int affectedRows =
            await _dbContext.Database
                .ExecuteSqlInterpolatedAsync(
                    $"""
                    UPDATE "InboxMessages"
                    SET
                        "Status" = {(int)InboxMessageStatus.Pending},
                        "NextAttemptAtUtc" = {nextAttemptAtUtc},
                        "LockId" = NULL,
                        "LockedUntilUtc" = NULL,
                        "LastError" = {normalizedError}
                    WHERE "Id" = {messageId}
                    AND "Status" = {(int)InboxMessageStatus.Processing}
                    AND "LockId" = {lockId};
                    """,
                    cancellationToken);

        return affectedRows == 1;
    }

    public async Task<bool> MarkFailedAsync(
        Guid messageId,
        Guid lockId,
        string error,
        CancellationToken cancellationToken = default)
    {
        ValidateOwnershipArguments(
            messageId,
            lockId);

        string normalizedError =
            NormalizeError(error);

        int affectedRows =
            await _dbContext.Database
                .ExecuteSqlInterpolatedAsync(
                    $"""
                    UPDATE "InboxMessages"
                    SET
                        "Status" = {(int)InboxMessageStatus.Failed},
                        "LockId" = NULL,
                        "LockedUntilUtc" = NULL,
                        "LastError" = {normalizedError}
                    WHERE "Id" = {messageId}
                    AND "Status" = {(int)InboxMessageStatus.Processing}
                    AND "LockId" = {lockId};
                    """,
                    cancellationToken);

        return affectedRows == 1;
    }

    private static void ValidateClaimArguments(
        Guid lockId,
        DateTime nowUtc,
        DateTime lockedUntilUtc,
        int batchSize)
    {
        if (lockId == Guid.Empty)
        {
            throw new ArgumentException(
                "Inbox lock id cannot be empty.",
                nameof(lockId));
        }

        EnsureUtc(
            nowUtc,
            nameof(nowUtc),
            "Inbox current time must be UTC.");

        EnsureUtc(
            lockedUntilUtc,
            nameof(lockedUntilUtc),
            "Inbox lock expiration time must be UTC.");

        if (lockedUntilUtc <= nowUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lockedUntilUtc),
                lockedUntilUtc,
                "Inbox lock expiration time must be later " +
                "than the current time.");
        }

        if (batchSize is <= 0 or > 1000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                batchSize,
                "Inbox batch size must be between 1 and 1000.");
        }
    }

    private static void ValidateOwnershipArguments(
        Guid messageId,
        Guid lockId)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException(
                "Inbox message id cannot be empty.",
                nameof(messageId));
        }

        if (lockId == Guid.Empty)
        {
            throw new ArgumentException(
                "Inbox lock id cannot be empty.",
                nameof(lockId));
        }
    }

    private static void EnsureUtc(
        DateTime value,
        string parameterName,
        string errorMessage)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                errorMessage,
                parameterName);
        }
    }

    private static string NormalizeError(
        string? error)
    {
        const int maximumLength = 2000;

        if (string.IsNullOrWhiteSpace(error))
        {
            return "Unknown Inbox processing error.";
        }

        string normalized = error.Trim();

        return normalized.Length <= maximumLength
            ? normalized
            : normalized[..maximumLength];
    }
}