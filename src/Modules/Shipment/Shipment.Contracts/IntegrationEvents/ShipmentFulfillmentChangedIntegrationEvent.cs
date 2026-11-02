using BuildingBlocks.Messaging.Abstractions.Events;

namespace Shipment.Contracts.IntegrationEvents;

public sealed record ShipmentFulfillmentChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid StoreId,
    Guid OrderId,
    Guid ShipmentId,
    string ShipmentNumber,
    ShipmentFulfillmentStatus Status,
    long FulfillmentRevision) : IIntegrationEvent
{
    public const string EventType = "shipment.fulfillment-changed.v1";
    string IIntegrationEvent.EventType => EventType;
}