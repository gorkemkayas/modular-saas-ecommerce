using BuildingBlocks.Domain.Events;
using Shipment.Domain.Enums;

namespace Shipment.Domain.Events;

public sealed record ShipmentFulfillmentChangedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid StoreId,
    Guid OrderId,
    Guid ShipmentId,
    string ShipmentNumber,
    ShipmentStatus Status) : IDomainEvent;
