using Shipment.Domain.Enums;

namespace Shipment.Contracts;

public sealed record ShipmentDetailResult(
    Guid ShipmentId,
    Guid StoreId,
    Guid OrderId,
    string OrderNumber,
    string ShipmentNumber,
    ShipmentStatus Status,
    string RecipientName,
    string RecipientPhoneNumber,
    ShipmentAddressResult DestinationAddress,
    string? CarrierCode,
    string? CarrierName,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl,
    string? InternalNote,
    string? CancellationReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? ReadyForDispatchAtUtc,
    DateTime? ShippedAtUtc,
    DateTime? DeliveredAtUtc,
    DateTime? CancelledAtUtc,
    IReadOnlyCollection<ShipmentLineResult> Lines,
    IReadOnlyCollection<ShipmentPackageResult> Packages);

public sealed record ShipmentAddressResult(
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode);

public sealed record ShipmentLineResult(
    Guid ShipmentLineId,
    Guid OrderItemId,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string? Sku,
    int Quantity);

public sealed record ShipmentPackageResult(
    Guid ShipmentPackageId,
    string PackageNumber,
    string? TrackingNumber,
    decimal? Weight,
    string? WeightUnit,
    string? LabelReference,
    DateTime CreatedAtUtc,
    DateTime? ShippedAtUtc,
    IReadOnlyCollection<ShipmentTrackingEventResult> TrackingEvents);

public sealed record ShipmentTrackingEventResult(
    Guid TrackingEventId,
    TrackingEventType Type,
    DateTime OccurredAtUtc,
    string? Location,
    string Description,
    string? RawStatusCode,
    string? RawStatusText);
