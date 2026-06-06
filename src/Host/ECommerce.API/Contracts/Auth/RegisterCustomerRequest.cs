namespace ECommerce.API.Contracts.Auth;

public sealed record RegisterCustomerRequest(
    string StoreSlug,
    string Email,
    string Password,
    string FirstName,
    string LastName);
