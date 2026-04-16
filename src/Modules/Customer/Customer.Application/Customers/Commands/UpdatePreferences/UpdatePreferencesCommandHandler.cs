using Customer.Application.Abstractions;
using Customer.Application.Exceptions;
using Customer.Domain.Repositories;
using MediatR;

namespace Customer.Application.Customers.Commands.UpdatePreferences;

public sealed class UpdatePreferencesCommandHandler : IRequestHandler<UpdatePreferencesCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePreferencesCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatePreferencesCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByExternalUserIdAsync(command.TenantId, command.ExternalUserId, cancellationToken)
            ?? throw new CustomerNotFoundException(command.TenantId, command.ExternalUserId, byExternalUserId: true);

        customer.UpdatePreferences(command.PreferredLanguage, command.PreferredCurrency);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
