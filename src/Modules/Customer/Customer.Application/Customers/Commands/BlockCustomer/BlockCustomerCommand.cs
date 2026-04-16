using MediatR;

namespace Customer.Application.Customers.Commands.BlockCustomer;

public sealed record BlockCustomerCommand(Guid TenantId, Guid CustomerId) : IRequest;
