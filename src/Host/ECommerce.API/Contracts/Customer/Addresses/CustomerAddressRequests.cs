using Customer.Domain.Enums;

namespace ECommerce.API.Contracts.Customer.Addresses;

public sealed record AddCustomerAddressRequest(
    AddressType AddressType,
    string Title,
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode,
    bool IsDefaultShipping,
    bool IsDefaultBilling);

public sealed record UpdateCustomerAddressRequest(
    AddressType AddressType,
    string Title,
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode);
