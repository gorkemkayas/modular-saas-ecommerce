namespace Customer.Contracts;

public sealed record CustomerAddressResult(
    Guid AddressId,
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
