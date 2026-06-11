namespace Shipment.Application.ShippingCarriers.DTOs;

public sealed record ShippingCarrierDto(
    Guid Id,
    Guid StoreId,
    string Code,
    string Name,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl,
    bool IsActive,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
