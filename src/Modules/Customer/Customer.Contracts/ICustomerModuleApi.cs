namespace Customer.Contracts;

public interface ICustomerModuleApi
{
    Task<CustomerProfileResult?> GetCustomerByExternalUserIdAsync(
        GetCustomerByExternalUserIdRequest request,
        CancellationToken cancellationToken = default);
}
