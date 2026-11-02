namespace BuildingBlocks.Infrastructure.Messaging.Inbox;

public sealed class InboxMessage
{
    private InboxMessage() { }

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

public enum InboxMessageStatus { Pending = 0, Processing = 1, Processed = 2, Failed = 3 }
