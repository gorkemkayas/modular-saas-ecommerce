using Customer.Application.Abstractions;
using Customer.Application.Exceptions;
using Customer.Domain.Repositories;
using Customer.Domain.ValueObjects;
using MediatR;

namespace Customer.Application.Customers.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMyProfileCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByExternalUserIdAsync(command.TenantId, command.ExternalUserId, cancellationToken)
            ?? throw new CustomerNotFoundException(command.TenantId, command.ExternalUserId, byExternalUserId: true);

        customer.UpdateProfile(
            PersonName.Create(command.FirstName, command.LastName),
            PhoneNumber.CreateNullable(command.PhoneNumber));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
