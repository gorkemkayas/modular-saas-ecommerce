using Customer.Application.Abstractions;
using Customer.Domain.Entities;
using Customer.Domain.Repositories;
using Customer.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Customer.Application.Customers.Commands.SyncCustomerFromIdentity;

public sealed class SyncCustomerFromIdentityCommandHandler
    : IRequestHandler<SyncCustomerFromIdentityCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SyncCustomerFromIdentityCommandHandler> _logger;

    public SyncCustomerFromIdentityCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<SyncCustomerFromIdentityCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(SyncCustomerFromIdentityCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email);
        var name = PersonName.Create(command.FirstName, command.LastName);

        var existingCustomer = await _customerRepository.GetByExternalUserIdAsync(
            command.TenantId,
            command.ExternalUserId,
            cancellationToken);

        if (existingCustomer is null)
        {
            var customer = Domain.Entities.Customer.Create(
                command.TenantId,
                command.ExternalUserId,
                email,
                name);

            await _customerRepository.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Customer provisioned from identity | CustomerId: {CustomerId} | TenantId: {TenantId} | ExternalUserId: {ExternalUserId}",
                customer.Id,
                command.TenantId,
                command.ExternalUserId);

            return customer.Id;
        }

        existingCustomer.SyncIdentity(email, name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer synchronized from identity | CustomerId: {CustomerId} | TenantId: {TenantId} | ExternalUserId: {ExternalUserId}",
            existingCustomer.Id,
            command.TenantId,
            command.ExternalUserId);

        return existingCustomer.Id;
    }
}
