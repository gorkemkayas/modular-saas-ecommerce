using Shipment.Domain.Enums;

namespace Shipment.Contracts;

public sealed record ShipmentSummaryResult(
    Guid ShipmentId,
    Guid OrderId,
    string OrderNumber,
    string ShipmentNumber,
    ShipmentStatus Status,
    string RecipientName,
    string? CarrierName,
    string? TrackingNumber,
    DateTime CreatedAtUtc,
    DateTime? ShippedAtUtc,
    DateTime? DeliveredAtUtc);
