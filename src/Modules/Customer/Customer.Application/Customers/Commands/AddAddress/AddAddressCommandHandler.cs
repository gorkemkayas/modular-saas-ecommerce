using Customer.Application.Abstractions;
using Customer.Application.Exceptions;
using Customer.Domain.Repositories;
using Customer.Domain.ValueObjects;
using MediatR;

namespace Customer.Application.Customers.Commands.AddAddress;

public sealed class AddAddressCommandHandler : IRequestHandler<AddAddressCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddAddressCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(AddAddressCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByExternalUserIdAsync(command.TenantId, command.ExternalUserId, cancellationToken)
            ?? throw new CustomerNotFoundException(command.TenantId, command.ExternalUserId, byExternalUserId: true);

        var addressId = customer.AddAddress(
            command.AddressType,
            command.Title,
            command.ContactName,
            PhoneNumber.Create(command.PhoneNumber),
            command.Country,
            command.City,
            command.District,
            command.Line1,
            command.Line2,
            command.PostalCode,
            command.IsDefaultShipping,
            command.IsDefaultBilling);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return addressId;
    }
}
