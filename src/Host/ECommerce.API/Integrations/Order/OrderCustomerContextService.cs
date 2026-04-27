using Customer.Application.Customers.DTOs;
using Customer.Application.Customers.Queries.GetMyProfile;
using MediatR;
using Order.Application.Integrations;

namespace ECommerce.API.Integrations.Order;

public sealed class OrderCustomerContextService : IOrderCustomerContextService
{
    private readonly ISender _sender;

    public OrderCustomerContextService(ISender sender)
    {
        _sender = sender;
    }

    public async Task<OrderCustomerIdentity?> GetCustomerIdentityAsync(
        Guid storeId,
        Guid externalUserId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _sender.Send(new GetMyProfileQuery(storeId, externalUserId), cancellationToken);
        return customer is null ? null : new OrderCustomerIdentity(customer.Id);
    }

    public async Task<OrderCustomerContext?> GetCustomerContextAsync(
        Guid storeId,
        Guid externalUserId,
        Guid shippingAddressId,
        Guid? billingAddressId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _sender.Send(new GetMyProfileQuery(storeId, externalUserId), cancellationToken);
        if (customer is null)
            return null;

        var shippingAddress = customer.Addresses.FirstOrDefault(x => x.Id == shippingAddressId);
        if (shippingAddress is null)
            return null;

        var billingAddress = billingAddressId.HasValue
            ? customer.Addresses.FirstOrDefault(x => x.Id == billingAddressId.Value)
            : customer.Addresses.FirstOrDefault(x => x.IsDefaultBilling) ?? shippingAddress;

        if (billingAddress is null)
            return null;

        var fullName = $"{customer.FirstName} {customer.LastName}".Trim();

        return new OrderCustomerContext(
            customer.Id,
            customer.Email,
            fullName,
            customer.PhoneNumber,
            customer.Preferences.PreferredCurrency,
            MapAddress(shippingAddress),
            MapAddress(billingAddress));
    }

    private static OrderAddressSnapshotData MapAddress(CustomerAddressDto address)
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
