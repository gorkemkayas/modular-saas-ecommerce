namespace Notification.Infrastructure.Persistence.Dispatch;

// Operational delivery state belongs to Infrastructure, not the dispatch aggregate.
public sealed class NotificationDispatchDelivery
{
    private NotificationDispatchDelivery() { }

    public NotificationDispatchDelivery(Guid dispatchId, DateTime nowUtc)
    {
        if (dispatchId == Guid.Empty)
            throw new ArgumentException("Dispatch id is required.", nameof(dispatchId));
        if (nowUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Delivery creation time must be UTC.", nameof(nowUtc));

        Id = dispatchId;
        Status = NotificationDispatchDeliveryStatus.Pending;
        CreatedAtUtc = nowUtc;
        NextAttemptAtUtc = nowUtc;
    }

    public Guid Id { get; private set; }
    public NotificationDispatchDeliveryStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime NextAttemptAtUtc { get; private set; }
    public int AttemptCount { get; private set; }
    public Guid? LockId { get; private set; }
    public DateTime? LockedUntilUtc { get; private set; }
    public string? Payload { get; private set; }
    public string? ProviderName { get; private set; }
    public DateTime? PreparedAtUtc { get; private set; }
    public string? LastError { get; private set; }
}
