using MediatR;

namespace Shipment.Application.ShippingCarriers.Commands.CreateShippingCarrier;

public sealed record CreateShippingCarrierCommand(
    Guid StoreId,
    string Code,
    string Name,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl,
    int SortOrder) : IRequest<Guid>;
