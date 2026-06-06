using MediatR;

namespace Shipment.Application.ShippingCarriers.Commands.UpdateShippingCarrier;

public sealed record UpdateShippingCarrierCommand(
    Guid StoreId,
    Guid CarrierId,
    string Code,
    string Name,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl,
    bool IsActive,
    int SortOrder) : IRequest;
