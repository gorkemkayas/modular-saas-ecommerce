using Shipment.Domain.Enums;

namespace Shipment.Application.Shipments.DTOs;

public sealed record ShipmentSummaryDto(
    Guid Id,
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

public sealed record ShipmentDto(
    Guid Id,
    Guid StoreId,
    Guid OrderId,
    string OrderNumber,
    string ShipmentNumber,
    ShipmentStatus Status,
    string RecipientName,
    string RecipientPhoneNumber,
    ShipmentAddressDto DestinationAddress,
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
    IReadOnlyCollection<ShipmentLineDto> Lines,
    IReadOnlyCollection<ShipmentPackageDto> Packages);

public sealed record ShipmentAddressDto(
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode);

public sealed record ShipmentLineDto(
    Guid Id,
    Guid OrderItemId,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string? Sku,
    int Quantity);

public sealed record ShipmentPackageDto(
    Guid Id,
    string PackageNumber,
    string? TrackingNumber,
    decimal? Weight,
    string? WeightUnit,
    string? LabelReference,
    DateTime CreatedAtUtc,
    DateTime? ShippedAtUtc,
    IReadOnlyCollection<TrackingEventDto> TrackingEvents);

public sealed record TrackingEventDto(
    Guid Id,
    TrackingEventType Type,
    DateTime OccurredAtUtc,
    string? Location,
    string Description,
    string? RawStatusCode,
    string? RawStatusText);

public sealed record ShipmentSearchCriteria(
    Guid StoreId,
    ShipmentStatus? Status,
    Guid? OrderId,
    string? OrderNumber,
    string? ShipmentNumber,
    string? TrackingNumber,
    int PageNumber,
    int PageSize);
