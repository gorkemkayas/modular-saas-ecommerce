using Customer.Application.Abstractions;
using Customer.Application.Exceptions;
using Customer.Domain.Repositories;
using Customer.Domain.ValueObjects;
using MediatR;

namespace Customer.Application.Customers.Commands.UpdateAddress;

public sealed class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAddressCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateAddressCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByExternalUserIdAsync(command.TenantId, command.ExternalUserId, cancellationToken)
            ?? throw new CustomerNotFoundException(command.TenantId, command.ExternalUserId, byExternalUserId: true);

        customer.UpdateAddress(
            command.AddressId,
            command.AddressType,
            command.Title,
            command.ContactName,
            PhoneNumber.Create(command.PhoneNumber),
            command.Country,
            command.City,
            command.District,
            command.Line1,
            command.Line2,
            command.PostalCode);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
