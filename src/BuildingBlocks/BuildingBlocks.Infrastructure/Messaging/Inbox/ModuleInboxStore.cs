using BuildingBlocks.Messaging.Abstractions.Publishing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox;

public sealed class ModuleInboxStore<TContext>(TContext dbContext) : IModuleInboxStore<TContext>
    where TContext : DbContext
{
    public async Task<bool> WriteAsync(IntegrationMessage message, DateTime receivedOnUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Id == Guid.Empty || string.IsNullOrWhiteSpace(message.EventType) ||
            message.EventType.Length > 200 || string.IsNullOrWhiteSpace(message.RoutingKey) ||
            message.RoutingKey.Length > 200 || string.IsNullOrWhiteSpace(message.Payload))
            throw new InvalidDataException("Inbox message identity, type, routing key and payload are required.");
        EnsureUtc(message.OccurredOnUtc);
        EnsureUtc(receivedOnUtc);

        int rows = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "InboxMessages"
                ("Id", "EventType", "RoutingKey", "Payload", "OccurredOnUtc", "ReceivedOnUtc",
                 "Status", "AttemptCount", "NextAttemptAtUtc")
            VALUES ({message.Id}, {message.EventType}, {message.RoutingKey}, CAST({message.Payload} AS jsonb),
                {message.OccurredOnUtc}, {receivedOnUtc}, {(int)InboxMessageStatus.Pending}, 0, {receivedOnUtc})
            ON CONFLICT ("Id") DO NOTHING;
            """, cancellationToken);
        return rows == 1;
    }

    public async Task<IReadOnlyList<ClaimedInboxMessage>> ClaimAsync(Guid lockId, DateTime nowUtc,
        DateTime lockedUntilUtc, int batchSize, CancellationToken cancellationToken = default)
    {
        if (lockId == Guid.Empty) throw new ArgumentException("Inbox lock identity cannot be empty.", nameof(lockId));
        EnsureUtc(nowUtc);
        EnsureUtc(lockedUntilUtc);
        if (lockedUntilUtc <= nowUtc) throw new ArgumentOutOfRangeException(nameof(lockedUntilUtc));
        if (batchSize is <= 0 or > 1000) throw new ArgumentOutOfRangeException(nameof(batchSize));
        const string sql = """
            WITH candidates AS (
                SELECT "Id" FROM "InboxMessages"
                WHERE ("Status" = 0 AND "NextAttemptAtUtc" <= @now)
                   OR ("Status" = 1 AND "LockedUntilUtc" <= @now)
                ORDER BY "ReceivedOnUtc", "Id" LIMIT @batch FOR UPDATE SKIP LOCKED
            )
            UPDATE "InboxMessages" AS inbox
            SET "Status" = 1, "AttemptCount" = inbox."AttemptCount" + 1,
                "LockId" = @lock, "LockedUntilUtc" = @until
            FROM candidates WHERE inbox."Id" = candidates."Id"
            RETURNING inbox."Id", inbox."LockId", inbox."EventType", inbox."RoutingKey",
                      inbox."Payload", inbox."OccurredOnUtc", inbox."AttemptCount";
            """;
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = new NpgsqlCommand(sql,
                (NpgsqlConnection)dbContext.Database.GetDbConnection(),
                (NpgsqlTransaction?)dbContext.Database.CurrentTransaction?.GetDbTransaction());
            command.Parameters.AddWithValue("now", nowUtc);
            command.Parameters.AddWithValue("until", lockedUntilUtc);
            command.Parameters.AddWithValue("batch", batchSize);
            command.Parameters.AddWithValue("lock", lockId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var result = new List<ClaimedInboxMessage>();
            while (await reader.ReadAsync(cancellationToken))
                result.Add(new(reader.GetGuid(0), reader.GetGuid(1), reader.GetString(2),
                    reader.GetString(3), reader.GetString(4), reader.GetDateTime(5), reader.GetInt32(6)));
            return result;
        }
        finally { await dbContext.Database.CloseConnectionAsync(); }
    }

    public async Task<bool> RenewLeaseAsync(Guid messageId, Guid lockId, DateTime nowUtc,
        DateTime lockedUntilUtc, CancellationToken cancellationToken = default)
    {
        ValidateLease(messageId, lockId, nowUtc, lockedUntilUtc);
        int rows = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "InboxMessages" SET "LockedUntilUtc" = {lockedUntilUtc}
            WHERE "Id" = {messageId} AND "LockId" = {lockId} AND "Status" = 1
                AND "LockedUntilUtc" > {nowUtc};
            """, cancellationToken);
        return rows == 1;
    }

    public async Task<bool> MarkProcessedAsync(Guid messageId, Guid lockId, DateTime processedOnUtc,
        CancellationToken cancellationToken = default)
    {
        ValidateOwnership(messageId, lockId);
        EnsureUtc(processedOnUtc);
        int rows = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "InboxMessages"
            SET "Status" = 2, "ProcessedOnUtc" = {processedOnUtc}, "LastError" = NULL,
                "LockId" = NULL, "LockedUntilUtc" = NULL
            WHERE "Id" = {messageId} AND "LockId" = {lockId} AND "Status" = 1;
            """, cancellationToken);
        return rows == 1;
    }

    public async Task<bool> ScheduleRetryAsync(Guid messageId, Guid lockId, DateTime nextAttemptAtUtc,
        string error, CancellationToken cancellationToken = default)
    {
        ValidateOwnership(messageId, lockId);
        EnsureUtc(nextAttemptAtUtc);
        string normalizedError = NormalizeError(error);
        int rows = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "InboxMessages"
            SET "Status" = 0, "NextAttemptAtUtc" = {nextAttemptAtUtc}, "LastError" = {normalizedError},
                "LockId" = NULL, "LockedUntilUtc" = NULL
            WHERE "Id" = {messageId} AND "LockId" = {lockId} AND "Status" = 1;
            """, cancellationToken);
        return rows == 1;
    }

    public async Task<bool> MarkFailedAsync(Guid messageId, Guid lockId, string error,
        CancellationToken cancellationToken = default)
    {
        ValidateOwnership(messageId, lockId);
        string normalizedError = NormalizeError(error);
        int rows = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "InboxMessages"
            SET "Status" = 3, "LastError" = {normalizedError}, "LockId" = NULL, "LockedUntilUtc" = NULL
            WHERE "Id" = {messageId} AND "LockId" = {lockId} AND "Status" = 1;
            """, cancellationToken);
        return rows == 1;
    }

    private static void ValidateOwnership(Guid messageId, Guid lockId)
    {
        if (messageId == Guid.Empty || lockId == Guid.Empty)
            throw new ArgumentException("Inbox message and lock identities cannot be empty.");
    }

    private static void ValidateLease(Guid messageId, Guid lockId, DateTime nowUtc, DateTime untilUtc)
    {
        ValidateOwnership(messageId, lockId);
        EnsureUtc(nowUtc);
        EnsureUtc(untilUtc);
        if (untilUtc <= nowUtc) throw new ArgumentOutOfRangeException(nameof(untilUtc));
    }

    private static void EnsureUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc) throw new ArgumentException("Inbox timestamps must be UTC.");
    }

    private static string NormalizeError(string error)
    {
        string result = string.IsNullOrWhiteSpace(error) ? "Unknown Inbox failure." : error.Trim();
        return result.Length <= 2000 ? result : result[..2000];
    }
}
