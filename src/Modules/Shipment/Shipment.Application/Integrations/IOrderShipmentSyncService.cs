namespace Shipment.Application.Integrations;

public interface IOrderShipmentSyncService
{
    Task MarkShipmentCreatedAsync(Guid storeId, Guid orderId, string shipmentReference, CancellationToken cancellationToken = default);
    Task MarkShippedAsync(Guid storeId, Guid orderId, string shipmentReference, CancellationToken cancellationToken = default);
    Task MarkDeliveredAsync(Guid storeId, Guid orderId, string shipmentReference, CancellationToken cancellationToken = default);
    Task MarkShipmentCancelledAsync(Guid storeId, Guid orderId, string shipmentReference, CancellationToken cancellationToken = default);
}
