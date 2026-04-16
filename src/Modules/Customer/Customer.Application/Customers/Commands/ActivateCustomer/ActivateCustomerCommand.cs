using MediatR;

namespace Customer.Application.Customers.Commands.ActivateCustomer;

public sealed record ActivateCustomerCommand(Guid TenantId, Guid CustomerId) : IRequest;
