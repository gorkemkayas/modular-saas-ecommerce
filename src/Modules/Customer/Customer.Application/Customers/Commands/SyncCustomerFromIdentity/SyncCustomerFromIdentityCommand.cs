using MediatR;

namespace Customer.Application.Customers.Commands.SyncCustomerFromIdentity;

public sealed record SyncCustomerFromIdentityCommand(
    Guid TenantId,
    Guid ExternalUserId,
    string Email,
    string FirstName,
    string LastName) : IRequest<Guid>;
