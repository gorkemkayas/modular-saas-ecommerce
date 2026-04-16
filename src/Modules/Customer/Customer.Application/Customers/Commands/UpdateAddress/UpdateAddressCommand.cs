using Customer.Domain.Enums;
using MediatR;

namespace Customer.Application.Customers.Commands.UpdateAddress;

public sealed record UpdateAddressCommand(
    Guid TenantId,
    Guid ExternalUserId,
    Guid AddressId,
    AddressType AddressType,
    string Title,
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode) : IRequest;
