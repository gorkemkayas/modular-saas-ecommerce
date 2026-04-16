using Customer.Domain.Entities;

namespace Customer.Domain.Repositories;

public interface ICustomerRepository
{
    Task AddAsync(Entities.Customer customer, CancellationToken cancellationToken = default);
    Task<Entities.Customer?> GetByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default);
    Task<Entities.Customer?> GetByExternalUserIdAsync(Guid tenantId, Guid externalUserId, CancellationToken cancellationToken = default);
}
