namespace Payment.Application.Integrations;

public interface IOrderPaymentContextService
{
    Task<OrderPaymentContext?> GetCustomerOrderContextAsync(
        Guid storeId,
        Guid externalUserId,
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<OrderPaymentContext?> GetStoreOrderContextAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken = default);
}
