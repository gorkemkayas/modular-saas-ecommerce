using Customer.Application.Abstractions.Queries;
using Customer.Application.Customers.DTOs;
using MediatR;

namespace Customer.Application.Customers.Queries.GetMyProfile;

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, CustomerDto?>
{
    private readonly ICustomerReadService _customerReadService;

    public GetMyProfileQueryHandler(ICustomerReadService customerReadService)
    {
        _customerReadService = customerReadService;
    }

    public Task<CustomerDto?> Handle(GetMyProfileQuery query, CancellationToken cancellationToken)
    {
        return _customerReadService.GetByExternalUserIdAsync(query.TenantId, query.ExternalUserId, cancellationToken);
    }
}
