using MediatR;

namespace Customer.Application.Customers.Commands.SetDefaultShippingAddress;

public sealed record SetDefaultShippingAddressCommand(
    Guid TenantId,
    Guid ExternalUserId,
    Guid AddressId) : IRequest;
