using MediatR;
using Shipment.Application.ShippingCarriers.DTOs;

namespace Shipment.Application.ShippingCarriers.Queries.ListShippingCarriers;

public sealed record ListShippingCarriersQuery(
    Guid StoreId,
    bool ActiveOnly) : IRequest<IReadOnlyCollection<ShippingCarrierDto>>;
