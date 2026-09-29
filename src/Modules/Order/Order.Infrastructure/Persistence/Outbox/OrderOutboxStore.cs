using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Order.Infrastructure.Persistence.Outbox;

public sealed class OrderOutboxStore : IOrderOutboxStore
{
    private const string ClaimSql =
        """
        WITH candidates AS
        (
            SELECT "Id"
            FROM "OutboxMessages"
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
            ORDER BY "CreatedAtUtc", "Id"
            LIMIT @batchSize
            FOR UPDATE SKIP LOCKED
        )
        UPDATE "OutboxMessages" AS outbox
        SET
            "Status" = @processingStatus,
            "AttemptCount" = outbox."AttemptCount" + 1,
            "LockId" = @lockId,
            "LockedUntilUtc" = @lockedUntilUtc
        FROM candidates
        WHERE outbox."Id" = candidates."Id"
        RETURNING
            outbox."Id",
            outbox."StoreId",
            outbox."EventType",
            outbox."RoutingKey",
            outbox."Payload",
            outbox."OccurredOnUtc",
            outbox."AttemptCount",
            outbox."LockId";
        """;

    private readonly OrderDbContext _dbContext;

    public OrderOutboxStore(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ClaimedOutboxMessage>> ClaimAsync(
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
                (NpgsqlConnection)_dbContext.Database.GetDbConnection();

            await using var command =
                new NpgsqlCommand(ClaimSql, connection);

            command.Parameters.AddWithValue(
                "pendingStatus",
                (int)OutboxMessageStatus.Pending);

            command.Parameters.AddWithValue(
                "processingStatus",
                (int)OutboxMessageStatus.Processing);

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

            var messages = new List<ClaimedOutboxMessage>();

            var idOrdinal = reader.GetOrdinal("Id");
            var storeIdOrdinal = reader.GetOrdinal("StoreId");
            var eventTypeOrdinal = reader.GetOrdinal("EventType");
            var routingKeyOrdinal = reader.GetOrdinal("RoutingKey");
            var payloadOrdinal = reader.GetOrdinal("Payload");
            var occurredOnUtcOrdinal =
                reader.GetOrdinal("OccurredOnUtc");
            var attemptCountOrdinal =
                reader.GetOrdinal("AttemptCount");
            var lockIdOrdinal = reader.GetOrdinal("LockId");

            while (await reader.ReadAsync(cancellationToken))
            {
                messages.Add(
                    new ClaimedOutboxMessage(
                        reader.GetGuid(idOrdinal),
                        reader.GetGuid(storeIdOrdinal),
                        reader.GetString(eventTypeOrdinal),
                        reader.GetString(routingKeyOrdinal),
                        reader.GetString(payloadOrdinal),
                        reader.GetDateTime(occurredOnUtcOrdinal),
                        reader.GetInt32(attemptCountOrdinal),
                        reader.GetGuid(lockIdOrdinal)));
            }

            return messages;
        }
        finally
        {
            await _dbContext.Database.CloseConnectionAsync();
        }
    }

    public async Task<bool> MarkPublishedAsync(
        Guid messageId,
        Guid lockId,
        DateTime publishedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ValidateOwnershipArguments(
            messageId,
            lockId);

        if (publishedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Outbox publication time must be UTC.",
                nameof(publishedAtUtc));
        }

        var affectedRows =
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE "OutboxMessages"
                SET
                    "Status" = {(int)OutboxMessageStatus.Published},
                    "PublishedAtUtc" = {publishedAtUtc},
                    "LockId" = NULL,
                    "LockedUntilUtc" = NULL,
                    "LastError" = NULL
                WHERE "Id" = {messageId}
                AND "Status" = {(int)OutboxMessageStatus.Processing}
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

        if (nextAttemptAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Outbox next attempt time must be UTC.",
                nameof(nextAttemptAtUtc));
        }

        var normalizedError = NormalizeError(error);

        var affectedRows =
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE "OutboxMessages"
                SET
                    "Status" = {(int)OutboxMessageStatus.Pending},
                    "NextAttemptAtUtc" = {nextAttemptAtUtc},
                    "LockId" = NULL,
                    "LockedUntilUtc" = NULL,
                    "LastError" = {normalizedError}
                WHERE "Id" = {messageId}
                AND "Status" = {(int)OutboxMessageStatus.Processing}
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

        var normalizedError = NormalizeError(error);

        var affectedRows =
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE "OutboxMessages"
                SET
                    "Status" = {(int)OutboxMessageStatus.Failed},
                    "LockId" = NULL,
                    "LockedUntilUtc" = NULL,
                    "LastError" = {normalizedError}
                WHERE "Id" = {messageId}
                AND "Status" = {(int)OutboxMessageStatus.Processing}
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
                "Outbox lock id cannot be empty.",
                nameof(lockId));
        }

        if (nowUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Outbox current time must be UTC.",
                nameof(nowUtc));
        }

        if (lockedUntilUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Outbox lock expiration time must be UTC.",
                nameof(lockedUntilUtc));
        }

        if (lockedUntilUtc <= nowUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lockedUntilUtc),
                lockedUntilUtc,
                "Outbox lock expiration time must be later " +
                "than the current time.");
        }

        if (batchSize is <= 0 or > 1000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                batchSize,
                "Outbox batch size must be between 1 and 1000.");
        }
    }

    private static void ValidateOwnershipArguments(
        Guid messageId,
        Guid lockId)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException(
                "Outbox message id cannot be empty.",
                nameof(messageId));
        }

        if (lockId == Guid.Empty)
        {
            throw new ArgumentException(
                "Outbox lock id cannot be empty.",
                nameof(lockId));
        }
    }

    private static string NormalizeError(string? error)
    {
        const int maximumLength = 2000;

        if (string.IsNullOrWhiteSpace(error))
        {
            return "Unknown Outbox publishing error.";
        }

        var normalized = error.Trim();

        return normalized.Length <= maximumLength
            ? normalized
            : normalized[..maximumLength];
    }
}