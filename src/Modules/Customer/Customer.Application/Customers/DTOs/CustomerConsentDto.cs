using Customer.Domain.Enums;

namespace Customer.Application.Customers.DTOs;

public sealed record CustomerConsentDto(
    ConsentType ConsentType,
    bool IsGranted,
    string Source,
    DateTime UpdatedAtUtc);
