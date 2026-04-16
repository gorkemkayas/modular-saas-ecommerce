namespace ECommerce.API.Contracts.Customer.Profile;

public sealed record UpdateMyCustomerProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber);
