using Customer.Domain.Enums;

namespace Customer.Application.Customers.DTOs;

public sealed record CustomerSearchCriteria(
    Guid TenantId,
    string? SearchTerm,
    CustomerStatus? Status,
    int PageNumber,
    int PageSize);
