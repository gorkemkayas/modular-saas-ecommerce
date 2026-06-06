namespace Order.Application.Orders.DTOs;

public sealed record OrderShippingCarrierSnapshotDto(
    Guid CarrierId,
    string Code,
    string Name,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl);
