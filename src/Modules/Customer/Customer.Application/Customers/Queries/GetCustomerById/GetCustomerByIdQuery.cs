using Customer.Application.Customers.DTOs;
using MediatR;

namespace Customer.Application.Customers.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid TenantId, Guid CustomerId) : IRequest<CustomerDto?>;
