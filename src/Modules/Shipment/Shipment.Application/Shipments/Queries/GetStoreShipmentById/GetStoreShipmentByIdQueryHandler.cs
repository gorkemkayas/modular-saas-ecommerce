using MediatR;
using Shipment.Application.Abstractions.Queries;
using Shipment.Application.Shipments.DTOs;

namespace Shipment.Application.Shipments.Queries.GetStoreShipmentById;

public sealed class GetStoreShipmentByIdQueryHandler : IRequestHandler<GetStoreShipmentByIdQuery, ShipmentDto?>
{
    private readonly IShipmentReadService _shipmentReadService;

    public GetStoreShipmentByIdQueryHandler(IShipmentReadService shipmentReadService)
    {
        _shipmentReadService = shipmentReadService;
    }

    public Task<ShipmentDto?> Handle(GetStoreShipmentByIdQuery query, CancellationToken cancellationToken)
    {
        return _shipmentReadService.GetByIdAsync(query.StoreId, query.ShipmentId, cancellationToken);
    }
}
