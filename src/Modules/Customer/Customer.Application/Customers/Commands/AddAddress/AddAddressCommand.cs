using Customer.Domain.Enums;
using MediatR;

namespace Customer.Application.Customers.Commands.AddAddress;

public sealed record AddAddressCommand(
    Guid TenantId,
    Guid ExternalUserId,
    AddressType AddressType,
    string Title,
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode,
    bool IsDefaultShipping,
    bool IsDefaultBilling) : IRequest<Guid>;
