namespace BuildingBlocks.Messaging.Abstractions.Events;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
    string EventType { get; }
}
