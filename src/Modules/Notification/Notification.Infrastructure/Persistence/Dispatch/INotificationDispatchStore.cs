namespace Notification.Infrastructure.Persistence.Dispatch;

public interface INotificationDispatchStore
{
    Task EnqueueAsync(Guid dispatchId, DateTime nowUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimedNotificationDispatch>> ClaimAsync(
        Guid lockId, DateTime nowUtc, DateTime lockedUntilUtc, int batchSize,
        CancellationToken cancellationToken = default);

    Task<bool> RenewLeaseAsync(
        Guid dispatchId, Guid lockId, DateTime nowUtc, DateTime lockedUntilUtc,
        CancellationToken cancellationToken = default);

    Task<bool> PrepareAsync(
        Guid dispatchId, Guid lockId, string payload, string providerName, DateTime nowUtc,
        CancellationToken cancellationToken = default);

    Task<bool> MarkCompletedAsync(Guid dispatchId, Guid lockId, CancellationToken cancellationToken = default);

    Task<bool> ScheduleRetryAsync(
        Guid dispatchId, Guid lockId, DateTime nextAttemptAtUtc, string error,
        CancellationToken cancellationToken = default);

    Task<bool> MarkFailedAsync(
        Guid dispatchId, Guid lockId, string error, CancellationToken cancellationToken = default);
}
