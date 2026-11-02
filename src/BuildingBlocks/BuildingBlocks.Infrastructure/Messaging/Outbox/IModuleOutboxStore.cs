using Microsoft.EntityFrameworkCore;
namespace BuildingBlocks.Infrastructure.Messaging.Outbox;

public interface IModuleOutboxStore<TContext> where TContext : DbContext
{
    Task<bool> RenewLeaseAsync(Guid messageId, Guid lockId, DateTime nowUtc,
        DateTime lockedUntilUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimedOutboxMessage>> ClaimAsync(
        Guid lockId,
        DateTime nowUtc,
        DateTime lockedUntilUtc,
        int batchSize,
        CancellationToken cancellationToken = default);

    Task<bool> MarkPublishedAsync(
        Guid messageId,
        Guid lockId,
        DateTime publishedAtUtc,
        CancellationToken cancellationToken = default);

    Task<bool> ScheduleRetryAsync(
        Guid messageId,
        Guid lockId,
        DateTime nextAttemptAtUtc,
        string error,
        CancellationToken cancellationToken = default);

    Task<bool> MarkFailedAsync(
        Guid messageId,
        Guid lockId,
        string error,
        CancellationToken cancellationToken = default);
}
