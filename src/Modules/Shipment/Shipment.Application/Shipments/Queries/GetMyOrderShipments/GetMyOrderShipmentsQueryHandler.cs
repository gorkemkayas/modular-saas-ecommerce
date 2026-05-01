using MediatR;
using Shipment.Application.Abstractions.Queries;
using Shipment.Application.Integrations;
using Shipment.Application.Shipments.DTOs;

namespace Shipment.Application.Shipments.Queries.GetMyOrderShipments;

public sealed class GetMyOrderShipmentsQueryHandler : IRequestHandler<GetMyOrderShipmentsQuery, IReadOnlyCollection<ShipmentSummaryDto>>
{
    private readonly IShipmentReadService _shipmentReadService;
    private readonly IOrderShipmentContextService _orderShipmentContextService;

    public GetMyOrderShipmentsQueryHandler(
        IShipmentReadService shipmentReadService,
        IOrderShipmentContextService orderShipmentContextService)
    {
        _shipmentReadService = shipmentReadService;
        _orderShipmentContextService = orderShipmentContextService;
    }

    public async Task<IReadOnlyCollection<ShipmentSummaryDto>> Handle(GetMyOrderShipmentsQuery query, CancellationToken cancellationToken)
    {
        var orderContext = await _orderShipmentContextService.GetCustomerOrderContextAsync(
            query.StoreId,
            query.ExternalUserId,
            query.OrderId,
            cancellationToken);

        return orderContext is null
            ? Array.Empty<ShipmentSummaryDto>()
            : await _shipmentReadService.ListByOrderIdAsync(query.StoreId, query.OrderId, cancellationToken);
    }
}
