namespace BuildingBlocks.Messaging.Abstractions.Publishing;

public sealed record IntegrationMessage(
    Guid Id,
    string EventType,
    string RoutingKey,
    string Payload,
    DateTime OccurredOnUtc);
