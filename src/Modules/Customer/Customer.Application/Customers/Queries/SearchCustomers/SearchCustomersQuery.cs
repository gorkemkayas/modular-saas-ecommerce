using Customer.Application.Common.Models;
using Customer.Application.Customers.DTOs;
using Customer.Domain.Enums;
using MediatR;

namespace Customer.Application.Customers.Queries.SearchCustomers;

public sealed record SearchCustomersQuery(
    Guid TenantId,
    string? SearchTerm,
    CustomerStatus? Status,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<CustomerSummaryDto>>;
