using Customer.Application.Abstractions.Queries;
using Customer.Application.Common.Models;
using Customer.Application.Customers.DTOs;
using MediatR;

namespace Customer.Application.Customers.Queries.SearchCustomers;

public sealed class SearchCustomersQueryHandler
    : IRequestHandler<SearchCustomersQuery, PagedResult<CustomerSummaryDto>>
{
    private readonly ICustomerReadService _customerReadService;

    public SearchCustomersQueryHandler(ICustomerReadService customerReadService)
    {
        _customerReadService = customerReadService;
    }

    public Task<PagedResult<CustomerSummaryDto>> Handle(SearchCustomersQuery query, CancellationToken cancellationToken)
    {
        var criteria = new CustomerSearchCriteria(
            query.TenantId,
            query.SearchTerm,
            query.Status,
            query.PageNumber <= 0 ? 1 : query.PageNumber,
            query.PageSize <= 0 ? 20 : query.PageSize);

        return _customerReadService.SearchAsync(criteria, cancellationToken);
    }
}
