using BuildingBlocks.Messaging.Abstractions.Events;

namespace Shipment.Contracts.IntegrationEvents;

public enum ShipmentEventKind { Created = 1, Shipped = 2, Delivered = 3, DeliveryException = 4 }

public sealed record ShipmentStatusChangedIntegrationEvent(
    Guid Id, DateTime OccurredOnUtc, Guid ShipmentId, Guid StoreId, Guid OrderId,
    Guid? CustomerId, string OrderNumber, string ShipmentNumber, ShipmentEventKind Kind,
    string? RecipientEmail, string? RecipientName, string? CarrierName,
    string? TrackingNumber, string? TrackingUrl, string? Description) : IIntegrationEvent
{
    public const string EventType = "shipment.status-changed.v1";
    string IIntegrationEvent.EventType => EventType;
}
