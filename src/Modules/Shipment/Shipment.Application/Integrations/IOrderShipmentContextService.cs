namespace Shipment.Application.Integrations;

public interface IOrderShipmentContextService
{
    Task<OrderShipmentContext?> GetCustomerOrderContextAsync(
        Guid storeId,
        Guid externalUserId,
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<OrderShipmentContext?> GetStoreOrderContextAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken = default);
}
