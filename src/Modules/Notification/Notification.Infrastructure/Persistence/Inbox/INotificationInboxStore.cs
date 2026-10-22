namespace Notification.Infrastructure.Persistence.Inbox;

public interface INotificationInboxStore
{
    Task<IReadOnlyList<ClaimedInboxMessage>> ClaimAsync(
        Guid lockId,
        DateTime nowUtc,
        DateTime lockedUntilUtc,
        int batchSize,
        CancellationToken cancellationToken = default);

    Task<bool> MarkProcessedAsync(
        Guid messageId,
        Guid lockId,
        DateTime processedOnUtc,
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