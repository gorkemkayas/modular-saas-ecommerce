namespace Order.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public OutboxMessage(
        Guid id,
        Guid storeId,
        string eventType,
        string routingKey,
        string payload,
        DateTime occurredOnUtc,
        DateTime createdAtUtc)
    {
        Id = id;
        StoreId = storeId;
        EventType = eventType;
        RoutingKey = routingKey;
        Payload = payload;
        OccurredOnUtc = occurredOnUtc;
        CreatedAtUtc = createdAtUtc;

        Status = OutboxMessageStatus.Pending;
        AttemptCount = 0;
        NextAttemptAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string EventType { get; private set; } = default!;
    public string RoutingKey { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public OutboxMessageStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime NextAttemptAtUtc { get; private set; }
    public Guid? LockId { get; private set; }
    public DateTime? LockedUntilUtc { get; private set; }
    public string? LastError { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }

}