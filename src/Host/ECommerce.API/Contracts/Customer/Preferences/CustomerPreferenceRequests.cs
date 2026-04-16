namespace ECommerce.API.Contracts.Customer.Preferences;

public sealed record UpdateCustomerPreferencesRequest(
    string? PreferredLanguage,
    string? PreferredCurrency);
