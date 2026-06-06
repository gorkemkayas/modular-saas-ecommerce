using Shipment.Domain.Enums;

namespace ECommerce.API.Contracts.Shipment;

public sealed record CreateShipmentRequest(string? InternalNote);

public sealed record CreateShippingCarrierRequest(
    string Code,
    string Name,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl,
    int SortOrder = 0);

public sealed record UpdateShippingCarrierRequest(
    string Code,
    string Name,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl,
    bool IsActive,
    int SortOrder);

public sealed record AddShipmentPackageRequest(
    string? TrackingNumber,
    decimal? Weight,
    string? WeightUnit,
    string? LabelReference);

public sealed record AssignShipmentCarrierRequest(
    string CarrierCode,
    string CarrierName,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl);

public sealed record RegisterShipmentTrackingEventRequest(
    Guid PackageId,
    TrackingEventType Type,
    DateTime OccurredAtUtc,
    string? Location,
    string Description,
    string? RawStatusCode,
    string? RawStatusText);

public sealed record MarkShipmentDeliveryExceptionRequest(
    Guid PackageId,
    string Description,
    string? Location,
    string? RawStatusCode,
    string? RawStatusText);

public sealed record CancelShipmentRequest(string? Reason);

public sealed record SearchShipmentsRequest(
    ShipmentStatus? Status,
    Guid? OrderId,
    string? OrderNumber,
    string? ShipmentNumber,
    string? TrackingNumber,
    int PageNumber = 1,
    int PageSize = 20);
