namespace Shipment.Contracts;

public sealed record ShippingCarrierResult(
    Guid Id,
    Guid StoreId,
    string Code,
    string Name,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl,
    bool IsActive,
    int SortOrder);
