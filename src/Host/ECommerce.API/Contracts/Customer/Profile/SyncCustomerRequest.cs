namespace ECommerce.API.Contracts.Customer.Profile;

public sealed record SyncCustomerRequest(
    Guid ExternalUserId,
    string Email,
    string FirstName,
    string LastName);
