namespace Order.Application.Integrations;

public interface IOrderShippingCarrierService
{
    Task<OrderShippingCarrier?> GetActiveCarrierAsync(
        Guid storeId,
        Guid carrierId,
        CancellationToken cancellationToken = default);
}
