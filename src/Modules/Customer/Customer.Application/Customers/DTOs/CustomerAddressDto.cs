using Customer.Domain.Enums;

namespace Customer.Application.Customers.DTOs;

public sealed record CustomerAddressDto(
    Guid Id,
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
