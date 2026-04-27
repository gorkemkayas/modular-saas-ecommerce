using Customer.Application.Abstractions.Queries;
using Customer.Contracts;

namespace Customer.Application.Contracts;

public sealed class CustomerModuleApi : ICustomerModuleApi
{
    private readonly ICustomerReadService _customerReadService;

    public CustomerModuleApi(ICustomerReadService customerReadService)
    {
        _customerReadService = customerReadService;
    }

    public async Task<CustomerProfileResult?> GetCustomerByExternalUserIdAsync(
        GetCustomerByExternalUserIdRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerReadService.GetByExternalUserIdAsync(
            request.TenantId,
            request.ExternalUserId,
            cancellationToken);

        if (customer is null)
            return null;

        return new CustomerProfileResult(
            customer.Id,
            customer.Email,
            customer.FirstName,
            customer.LastName,
            customer.PhoneNumber,
            customer.Preferences.PreferredCurrency,
            customer.Addresses
                .Select(address => new CustomerAddressResult(
                    address.Id,
                    address.Title,
                    address.ContactName,
                    address.PhoneNumber,
                    address.Country,
                    address.City,
                    address.District,
                    address.Line1,
                    address.Line2,
                    address.PostalCode,
                    address.IsDefaultShipping,
                    address.IsDefaultBilling))
                .ToArray());
    }
}
