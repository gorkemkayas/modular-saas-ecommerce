using MediatR;
using Shipment.Application.Abstractions.Queries;
using Shipment.Application.Integrations;
using Shipment.Application.Shipments.DTOs;

namespace Shipment.Application.Shipments.Queries.GetMyOrderShipmentById;

public sealed class GetMyOrderShipmentByIdQueryHandler : IRequestHandler<GetMyOrderShipmentByIdQuery, ShipmentDto?>
{
    private readonly IShipmentReadService _shipmentReadService;
    private readonly IOrderShipmentContextService _orderShipmentContextService;

    public GetMyOrderShipmentByIdQueryHandler(
        IShipmentReadService shipmentReadService,
        IOrderShipmentContextService orderShipmentContextService)
    {
        _shipmentReadService = shipmentReadService;
        _orderShipmentContextService = orderShipmentContextService;
    }

    public async Task<ShipmentDto?> Handle(GetMyOrderShipmentByIdQuery query, CancellationToken cancellationToken)
    {
        var orderContext = await _orderShipmentContextService.GetCustomerOrderContextAsync(
            query.StoreId,
            query.ExternalUserId,
            query.OrderId,
            cancellationToken);

        if (orderContext is null)
            return null;

        var shipment = await _shipmentReadService.GetByIdAsync(query.StoreId, query.ShipmentId, cancellationToken);
        return shipment is not null && shipment.OrderId == query.OrderId ? shipment : null;
    }
}
