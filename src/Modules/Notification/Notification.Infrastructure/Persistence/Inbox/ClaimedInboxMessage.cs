namespace Notification.Infrastructure.Persistence.Inbox;

public sealed record ClaimedInboxMessage(
    Guid Id,
    Guid LockId,
    string EventType,
    string RoutingKey,
    string Payload,
    DateTime OccurredOnUtc,
    int AttemptCount);