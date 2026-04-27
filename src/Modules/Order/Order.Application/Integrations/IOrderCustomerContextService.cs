namespace Order.Application.Integrations;

public interface IOrderCustomerContextService
{
    Task<OrderCustomerIdentity?> GetCustomerIdentityAsync(
        Guid storeId,
        Guid externalUserId,
        CancellationToken cancellationToken = default);

    Task<OrderCustomerContext?> GetCustomerContextAsync(
        Guid storeId,
        Guid externalUserId,
        Guid shippingAddressId,
        Guid? billingAddressId,
        CancellationToken cancellationToken = default);
}
