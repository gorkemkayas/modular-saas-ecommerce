namespace Customer.Contracts;

public sealed record CustomerProfileResult(
    Guid CustomerId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? PreferredCurrency,
    IReadOnlyCollection<CustomerAddressResult> Addresses);
