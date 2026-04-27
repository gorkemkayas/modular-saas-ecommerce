using Customer.Contracts;
using Order.Application.Integrations;

namespace Order.Infrastructure.Integrations.Customer;

public sealed class OrderCustomerContextService : IOrderCustomerContextService
{
    private readonly ICustomerModuleApi _customerModuleApi;

    public OrderCustomerContextService(ICustomerModuleApi customerModuleApi)
    {
        _customerModuleApi = customerModuleApi;
    }

    public async Task<OrderCustomerIdentity?> GetCustomerIdentityAsync(
        Guid storeId,
        Guid externalUserId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerModuleApi.GetCustomerByExternalUserIdAsync(
            new GetCustomerByExternalUserIdRequest(storeId, externalUserId),
            cancellationToken);

        return customer is null ? null : new OrderCustomerIdentity(customer.CustomerId);
    }

    public async Task<OrderCustomerContext?> GetCustomerContextAsync(
        Guid storeId,
        Guid externalUserId,
        Guid shippingAddressId,
        Guid? billingAddressId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerModuleApi.GetCustomerByExternalUserIdAsync(
            new GetCustomerByExternalUserIdRequest(storeId, externalUserId),
            cancellationToken);

        if (customer is null)
            return null;

        var shippingAddress = customer.Addresses.FirstOrDefault(x => x.AddressId == shippingAddressId);
        if (shippingAddress is null)
            return null;

        var billingAddress = billingAddressId.HasValue
            ? customer.Addresses.FirstOrDefault(x => x.AddressId == billingAddressId.Value)
            : customer.Addresses.FirstOrDefault(x => x.IsDefaultBilling) ?? shippingAddress;

        if (billingAddress is null)
            return null;

        var fullName = $"{customer.FirstName} {customer.LastName}".Trim();

        return new OrderCustomerContext(
            customer.CustomerId,
            customer.Email,
            fullName,
            customer.PhoneNumber,
            customer.PreferredCurrency,
            MapAddress(shippingAddress),
            MapAddress(billingAddress));
    }

    private static OrderAddressSnapshotData MapAddress(CustomerAddressResult address)
    {
        return new OrderAddressSnapshotData(
            address.Title,
            address.ContactName,
            address.PhoneNumber,
            address.Country,
            address.City,
            address.District,
            address.Line1,
            address.Line2,
            address.PostalCode);
    }
}
