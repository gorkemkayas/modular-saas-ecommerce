using Order.Contracts;
using Shipment.Application.Integrations;

namespace Shipment.Infrastructure.Integrations.Order;

public sealed class OrderShipmentSyncService : IOrderShipmentSyncService
{
    private readonly IOrderModuleApi _orderModuleApi;

    public OrderShipmentSyncService(IOrderModuleApi orderModuleApi)
    {
        _orderModuleApi = orderModuleApi;
    }

    public Task MarkShipmentCreatedAsync(Guid storeId, Guid orderId, string shipmentReference, CancellationToken cancellationToken = default)
    {
        return _orderModuleApi.MarkShipmentCreatedAsync(
            new UpdateOrderShipmentStatusRequest(storeId, orderId, shipmentReference),
            cancellationToken);
    }

    public Task MarkShippedAsync(Guid storeId, Guid orderId, string shipmentReference, CancellationToken cancellationToken = default)
    {
        return _orderModuleApi.MarkShippedAsync(
            new UpdateOrderShipmentStatusRequest(storeId, orderId, shipmentReference),
            cancellationToken);
    }

    public Task MarkDeliveredAsync(Guid storeId, Guid orderId, string shipmentReference, CancellationToken cancellationToken = default)
    {
        return _orderModuleApi.MarkDeliveredAsync(
            new UpdateOrderShipmentStatusRequest(storeId, orderId, shipmentReference),
            cancellationToken);
    }

    public Task MarkShipmentCancelledAsync(Guid storeId, Guid orderId, string shipmentReference, CancellationToken cancellationToken = default)
    {
        return _orderModuleApi.MarkShipmentCancelledAsync(
            new UpdateOrderShipmentStatusRequest(storeId, orderId, shipmentReference),
            cancellationToken);
    }
}
