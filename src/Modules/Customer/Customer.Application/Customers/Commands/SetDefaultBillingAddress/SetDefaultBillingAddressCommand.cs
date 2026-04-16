using MediatR;

namespace Customer.Application.Customers.Commands.SetDefaultBillingAddress;

public sealed record SetDefaultBillingAddressCommand(
    Guid TenantId,
    Guid ExternalUserId,
    Guid AddressId) : IRequest;
