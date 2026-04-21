using Microsoft.EntityFrameworkCore;
using Pricing.Application.Abstractions;
using Pricing.Domain.Entities;

namespace Pricing.Infrastructure.Persistence;

public sealed class PricingDbContext : DbContext, IUnitOfWork
{
    public PricingDbContext(DbContextOptions<PricingDbContext> options)
        : base(options)
    {
    }

    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceEntry> PriceEntries => Set<PriceEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
