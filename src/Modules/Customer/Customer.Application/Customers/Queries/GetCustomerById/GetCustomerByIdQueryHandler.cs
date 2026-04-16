using Customer.Application.Abstractions.Queries;
using Customer.Application.Customers.DTOs;
using MediatR;

namespace Customer.Application.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly ICustomerReadService _customerReadService;

    public GetCustomerByIdQueryHandler(ICustomerReadService customerReadService)
    {
        _customerReadService = customerReadService;
    }

    public Task<CustomerDto?> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
    {
        return _customerReadService.GetByIdAsync(query.TenantId, query.CustomerId, cancellationToken);
    }
}
