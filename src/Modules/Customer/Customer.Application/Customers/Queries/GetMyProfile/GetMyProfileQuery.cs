using Customer.Application.Customers.DTOs;
using MediatR;

namespace Customer.Application.Customers.Queries.GetMyProfile;

public sealed record GetMyProfileQuery(Guid TenantId, Guid ExternalUserId) : IRequest<CustomerDto?>;
