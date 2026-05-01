using MediatR;
using Shipment.Application.Abstractions.Queries;
using Shipment.Application.Shipments.DTOs;

namespace Shipment.Application.Shipments.Queries.GetStoreOrderShipments;

public sealed class GetStoreOrderShipmentsQueryHandler : IRequestHandler<GetStoreOrderShipmentsQuery, IReadOnlyCollection<ShipmentSummaryDto>>
{
    private readonly IShipmentReadService _shipmentReadService;

    public GetStoreOrderShipmentsQueryHandler(IShipmentReadService shipmentReadService)
    {
        _shipmentReadService = shipmentReadService;
    }

    public Task<IReadOnlyCollection<ShipmentSummaryDto>> Handle(GetStoreOrderShipmentsQuery query, CancellationToken cancellationToken)
    {
        return _shipmentReadService.ListByOrderIdAsync(query.StoreId, query.OrderId, cancellationToken);
    }
}
