namespace Notification.Infrastructure.Persistence.Inbox;

public sealed class InboxMessage
{
    private InboxMessage()
    {
    }

    public InboxMessage(
        Guid id,
        string eventType,
        string routingKey,
        string payload,
        DateTime occurredOnUtc,
        DateTime receivedOnUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Inbox message id cannot be empty.",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException(
                "Inbox event type is required.",
                nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(routingKey))
        {
            throw new ArgumentException(
                "Inbox routing key is required.",
                nameof(routingKey));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException(
                "Inbox payload is required.",
                nameof(payload));
        }

        if (occurredOnUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Inbox event occurrence time must be UTC.",
                nameof(occurredOnUtc));
        }

        if (receivedOnUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Inbox received time must be UTC.",
                nameof(receivedOnUtc));
        }

        Id = id;
        EventType = eventType.Trim();
        RoutingKey = routingKey.Trim();
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
        ReceivedOnUtc = receivedOnUtc;

        Status = InboxMessageStatus.Pending;
        AttemptCount = 0;
        NextAttemptAtUtc = receivedOnUtc;
    }

    public Guid Id { get; private set; }

    public string EventType { get; private set; } = default!;

    public string RoutingKey { get; private set; } = default!;

    public string Payload { get; private set; } = default!;

    public DateTime OccurredOnUtc { get; private set; }

    public DateTime ReceivedOnUtc { get; private set; }

    public InboxMessageStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTime NextAttemptAtUtc { get; private set; }

    public Guid? LockId { get; private set; }

    public DateTime? LockedUntilUtc { get; private set; }

    public string? LastError { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }
}
