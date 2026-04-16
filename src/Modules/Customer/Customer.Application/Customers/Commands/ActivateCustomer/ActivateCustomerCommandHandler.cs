using Customer.Application.Abstractions;
using Customer.Application.Exceptions;
using Customer.Domain.Repositories;
using MediatR;

namespace Customer.Application.Customers.Commands.ActivateCustomer;

public sealed class ActivateCustomerCommandHandler : IRequestHandler<ActivateCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(command.TenantId, command.CustomerId, cancellationToken)
            ?? throw new CustomerNotFoundException(command.TenantId, command.CustomerId);

        customer.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
