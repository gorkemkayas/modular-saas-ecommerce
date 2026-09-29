namespace Order.Infrastructure.Persistence.Outbox;

public sealed record ClaimedOutboxMessage(
    Guid Id,
    Guid StoreId,
    string EventType,
    string RoutingKey,
    string Payload,
    DateTime OccurredOnUtc,
    int AttemptCount,
    Guid LockId);