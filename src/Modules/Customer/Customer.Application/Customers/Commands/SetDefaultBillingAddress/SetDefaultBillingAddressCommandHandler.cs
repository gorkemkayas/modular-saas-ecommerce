using Customer.Application.Abstractions;
using Customer.Application.Exceptions;
using Customer.Domain.Repositories;
using MediatR;

namespace Customer.Application.Customers.Commands.SetDefaultBillingAddress;

public sealed class SetDefaultBillingAddressCommandHandler : IRequestHandler<SetDefaultBillingAddressCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetDefaultBillingAddressCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetDefaultBillingAddressCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByExternalUserIdAsync(command.TenantId, command.ExternalUserId, cancellationToken)
            ?? throw new CustomerNotFoundException(command.TenantId, command.ExternalUserId, byExternalUserId: true);

        customer.SetDefaultBillingAddress(command.AddressId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
