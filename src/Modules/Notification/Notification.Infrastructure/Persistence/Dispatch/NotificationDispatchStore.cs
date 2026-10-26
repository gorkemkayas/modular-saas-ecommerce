using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Notification.Infrastructure.Persistence.Dispatch;

public sealed class NotificationDispatchStore : INotificationDispatchStore
{
    private const string ClaimSql =
        """
        WITH candidates AS
        (
            SELECT "Id"
            FROM "NotificationDispatchDeliveries"
            WHERE ("Status" = @pending AND "NextAttemptAtUtc" <= @now)
               OR ("Status" = @processing AND "LockedUntilUtc" <= @now)
            ORDER BY "CreatedAtUtc", "Id"
            LIMIT @batchSize
            FOR UPDATE SKIP LOCKED
        )
        UPDATE "NotificationDispatchDeliveries" AS delivery
        SET "Status" = @processing,
            "AttemptCount" = delivery."AttemptCount" + 1,
            "LockId" = @lockId,
            "LockedUntilUtc" = @lockedUntil
        FROM candidates
        WHERE delivery."Id" = candidates."Id"
        RETURNING delivery."Id", delivery."AttemptCount", delivery."Payload",
                  delivery."ProviderName", delivery."PreparedAtUtc";
        """;

    private readonly NotificationDbContext _dbContext;

    public NotificationDispatchStore(NotificationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public Task EnqueueAsync(Guid dispatchId, DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.NotificationDispatchDeliveries.AddAsync(
            new NotificationDispatchDelivery(dispatchId, nowUtc), cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<ClaimedNotificationDispatch>> ClaimAsync(
        Guid lockId, DateTime nowUtc, DateTime lockedUntilUtc, int batchSize,
        CancellationToken cancellationToken = default)
    {
        ValidateId(lockId, nameof(lockId));
        ValidateLease(nowUtc, lockedUntilUtc);
        if (batchSize is <= 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(batchSize));

        await _dbContext.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            var connection = (NpgsqlConnection)_dbContext.Database.GetDbConnection();
            await using var command = new NpgsqlCommand(ClaimSql, connection);
            command.Parameters.AddWithValue("pending", (int)NotificationDispatchDeliveryStatus.Pending);
            command.Parameters.AddWithValue("processing", (int)NotificationDispatchDeliveryStatus.Processing);
            command.Parameters.AddWithValue("now", nowUtc);
            command.Parameters.AddWithValue("lockedUntil", lockedUntilUtc);
            command.Parameters.AddWithValue("batchSize", batchSize);
            command.Parameters.AddWithValue("lockId", lockId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var deliveries = new List<ClaimedNotificationDispatch>();
            while (await reader.ReadAsync(cancellationToken))
            {
                deliveries.Add(new ClaimedNotificationDispatch(
                    reader.GetGuid(0), lockId, reader.GetInt32(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3),
                    reader.IsDBNull(4) ? null : reader.GetDateTime(4)));
            }
            return deliveries;
        }
        finally
        {
            await _dbContext.Database.CloseConnectionAsync();
        }
    }

    public async Task<bool> RenewLeaseAsync(
        Guid dispatchId, Guid lockId, DateTime nowUtc, DateTime lockedUntilUtc,
        CancellationToken cancellationToken = default)
    {
        ValidateOwnership(dispatchId, lockId);
        ValidateLease(nowUtc, lockedUntilUtc);
        int affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE "NotificationDispatchDeliveries"
            SET "LockedUntilUtc" = {lockedUntilUtc}
            WHERE "Id" = {dispatchId}
              AND "Status" = {(int)NotificationDispatchDeliveryStatus.Processing}
              AND "LockId" = {lockId}
              AND "LockedUntilUtc" > {nowUtc};
            """, cancellationToken);
        return affected == 1;
    }

    public async Task<bool> PrepareAsync(
        Guid dispatchId, Guid lockId, string payload, string providerName, DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ValidateOwnership(dispatchId, lockId);
        ValidateUtc(nowUtc);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);

        // Freeze the provider request before the first network call.
        int affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE "NotificationDispatchDeliveries"
            SET "Payload" = COALESCE("Payload", {payload}),
                "ProviderName" = COALESCE("ProviderName", {providerName}),
                "PreparedAtUtc" = COALESCE("PreparedAtUtc", {nowUtc})
            WHERE "Id" = {dispatchId}
              AND "Status" = {(int)NotificationDispatchDeliveryStatus.Processing}
              AND "LockId" = {lockId}
              AND "LockedUntilUtc" > {nowUtc};
            """, cancellationToken);
        return affected == 1;
    }

    public Task<bool> MarkCompletedAsync(
        Guid dispatchId, Guid lockId, CancellationToken cancellationToken = default)
    {
        return TransitionAsync(dispatchId, lockId,
            NotificationDispatchDeliveryStatus.Completed, null, null, cancellationToken);
    }

    public Task<bool> ScheduleRetryAsync(
        Guid dispatchId, Guid lockId, DateTime nextAttemptAtUtc, string error,
        CancellationToken cancellationToken = default)
    {
        ValidateUtc(nextAttemptAtUtc);
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return TransitionAsync(dispatchId, lockId,
            NotificationDispatchDeliveryStatus.Pending, nextAttemptAtUtc, error, cancellationToken);
    }

    public Task<bool> MarkFailedAsync(
        Guid dispatchId, Guid lockId, string error, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return TransitionAsync(dispatchId, lockId,
            NotificationDispatchDeliveryStatus.Failed, null, error, cancellationToken);
    }

    private async Task<bool> TransitionAsync(
        Guid dispatchId, Guid lockId, NotificationDispatchDeliveryStatus status,
        DateTime? nextAttemptAtUtc, string? error, CancellationToken cancellationToken)
    {
        ValidateOwnership(dispatchId, lockId);
        string? normalizedError = error is { Length: > 2000 } ? error[..2000] : error;
        int affected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE "NotificationDispatchDeliveries"
            SET "Status" = {(int)status},
                "NextAttemptAtUtc" = COALESCE({nextAttemptAtUtc}, "NextAttemptAtUtc"),
                "LastError" = {normalizedError},
                "LockId" = NULL,
                "LockedUntilUtc" = NULL
            WHERE "Id" = {dispatchId}
              AND "Status" = {(int)NotificationDispatchDeliveryStatus.Processing}
              AND "LockId" = {lockId};
            """, cancellationToken);
        return affected == 1;
    }

    private static void ValidateOwnership(Guid dispatchId, Guid lockId)
    {
        ValidateId(dispatchId, nameof(dispatchId));
        ValidateId(lockId, nameof(lockId));
    }

    private static void ValidateId(Guid value, string name)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", name);
    }

    private static void ValidateLease(DateTime nowUtc, DateTime lockedUntilUtc)
    {
        ValidateUtc(nowUtc);
        ValidateUtc(lockedUntilUtc);
        if (lockedUntilUtc <= nowUtc)
            throw new ArgumentOutOfRangeException(nameof(lockedUntilUtc));
    }

    private static void ValidateUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Delivery timestamps must be UTC.", nameof(value));
    }
}
