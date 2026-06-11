using Microsoft.EntityFrameworkCore;
using Subscription.Application.Abstractions;
using Subscription.Domain.Entities;

namespace Subscription.Infrastructure.Persistence;

public sealed class SubscriptionDbContext : DbContext, IUnitOfWork
{
    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
    public DbSet<PlanQuota> PlanQuotas => Set<PlanQuota>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubscriptionDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
