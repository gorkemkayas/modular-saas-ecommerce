namespace Customer.Application.Customers.DTOs;

public sealed record CustomerPreferencesDto(
    string? PreferredLanguage,
    string? PreferredCurrency);
