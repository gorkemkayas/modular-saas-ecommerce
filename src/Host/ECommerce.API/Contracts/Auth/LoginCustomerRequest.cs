namespace ECommerce.API.Contracts.Auth;

public sealed record LoginCustomerRequest(
    string StoreSlug,
    string Email,
    string Password,
    bool IsPersistent,
    bool AllowInactiveStore = false);
