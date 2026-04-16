using MediatR;

namespace Customer.Application.Customers.Commands.RemoveAddress;

public sealed record RemoveAddressCommand(
    Guid TenantId,
    Guid ExternalUserId,
    Guid AddressId) : IRequest;
