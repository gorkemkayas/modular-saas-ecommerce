using MediatR;

namespace Customer.Application.Customers.Commands.UpdatePreferences;

public sealed record UpdatePreferencesCommand(
    Guid TenantId,
    Guid ExternalUserId,
    string? PreferredLanguage,
    string? PreferredCurrency) : IRequest;
