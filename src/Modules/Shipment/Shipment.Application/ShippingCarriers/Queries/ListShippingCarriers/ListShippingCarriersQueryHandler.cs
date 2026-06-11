using MediatR;
using Shipment.Application.Abstractions.Queries;
using Shipment.Application.Exceptions;
using Shipment.Application.ShippingCarriers.DTOs;

namespace Shipment.Application.ShippingCarriers.Queries.ListShippingCarriers;

public sealed class ListShippingCarriersQueryHandler : IRequestHandler<ListShippingCarriersQuery, IReadOnlyCollection<ShippingCarrierDto>>
{
    private readonly IShippingCarrierReadService _shippingCarrierReadService;

    public ListShippingCarriersQueryHandler(IShippingCarrierReadService shippingCarrierReadService)
    {
        _shippingCarrierReadService = shippingCarrierReadService;
    }

    public Task<IReadOnlyCollection<ShippingCarrierDto>> Handle(
        ListShippingCarriersQuery query,
        CancellationToken cancellationToken)
    {
        if (query.StoreId == Guid.Empty)
            throw new ShipmentValidationException("StoreId is required.");

        return _shippingCarrierReadService.ListAsync(query.StoreId, query.ActiveOnly, cancellationToken);
    }
}
