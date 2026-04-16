using Customer.Application.Abstractions;
using Customer.Application.Exceptions;
using Customer.Domain.Repositories;
using MediatR;

namespace Customer.Application.Customers.Commands.BlockCustomer;

public sealed class BlockCustomerCommandHandler : IRequestHandler<BlockCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BlockCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(BlockCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(command.TenantId, command.CustomerId, cancellationToken)
            ?? throw new CustomerNotFoundException(command.TenantId, command.CustomerId);

        customer.Block();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
