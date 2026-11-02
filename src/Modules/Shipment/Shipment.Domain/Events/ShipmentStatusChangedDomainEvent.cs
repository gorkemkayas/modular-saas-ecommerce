using BuildingBlocks.Domain.Events;

namespace Shipment.Domain.Events;

public enum ShipmentEventKind { Created = 1, Shipped = 2, Delivered = 3, DeliveryException = 4 }

public sealed record ShipmentStatusChangedDomainEvent(
    Guid Id, DateTime OccurredOnUtc, Guid ShipmentId, Guid StoreId, Guid OrderId,
    ShipmentEventKind Kind, string? Description = null) : IDomainEvent;
