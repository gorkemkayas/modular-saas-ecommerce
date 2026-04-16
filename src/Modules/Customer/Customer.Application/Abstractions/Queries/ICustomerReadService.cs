using Customer.Application.Common.Models;
using Customer.Application.Customers.DTOs;

namespace Customer.Application.Abstractions.Queries;

public interface ICustomerReadService
{
    Task<CustomerDto?> GetByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByExternalUserIdAsync(Guid tenantId, Guid externalUserId, CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerSummaryDto>> SearchAsync(CustomerSearchCriteria criteria, CancellationToken cancellationToken = default);
}
