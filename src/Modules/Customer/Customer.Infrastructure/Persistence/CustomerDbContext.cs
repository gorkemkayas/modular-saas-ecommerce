using Customer.Application.Abstractions;
using Customer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Customer.Infrastructure.Persistence;

public sealed class CustomerDbContext : DbContext, IUnitOfWork
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Domain.Entities.Customer> Customers => Set<Domain.Entities.Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerConsent> CustomerConsents => Set<CustomerConsent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerDbContext).Assembly);
    }
}
