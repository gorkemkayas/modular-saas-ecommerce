using Customer.Domain.Enums;

namespace Customer.Application.Customers.DTOs;

public sealed record CustomerSummaryDto(
    Guid Id,
    Guid ExternalUserId,
    string Email,
    string FullName,
    string? PhoneNumber,
    CustomerStatus Status,
    int AddressCount,
    DateTime RegisteredAtUtc,
    DateTime UpdatedAtUtc);
