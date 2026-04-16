using Microsoft.EntityFrameworkCore;

namespace Customer.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : Domain.Repositories.ICustomerRepository
{
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Domain.Entities.Customer customer, CancellationToken cancellationToken = default)
    {
        return _context.Customers.AddAsync(customer, cancellationToken).AsTask();
    }

    public Task<Domain.Entities.Customer?> GetByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return BuildAggregateQuery()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == customerId, cancellationToken);
    }

    public Task<Domain.Entities.Customer?> GetByExternalUserIdAsync(Guid tenantId, Guid externalUserId, CancellationToken cancellationToken = default)
    {
        return BuildAggregateQuery()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ExternalUserId == externalUserId, cancellationToken);
    }

    private IQueryable<Domain.Entities.Customer> BuildAggregateQuery()
    {
        return _context.Customers
            .Include(x => x.Addresses)
            .Include(x => x.Consents)
            .AsSplitQuery();
    }
}
