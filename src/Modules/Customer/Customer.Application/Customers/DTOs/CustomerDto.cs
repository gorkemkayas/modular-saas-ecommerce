using Customer.Domain.Enums;

namespace Customer.Application.Customers.DTOs;

public sealed record CustomerDto(
    Guid Id,
    Guid TenantId,
    Guid ExternalUserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    CustomerStatus Status,
    DateTime RegisteredAtUtc,
    DateTime UpdatedAtUtc,
    CustomerPreferencesDto Preferences,
    IReadOnlyCollection<CustomerAddressDto> Addresses,
    IReadOnlyCollection<CustomerConsentDto> Consents);
