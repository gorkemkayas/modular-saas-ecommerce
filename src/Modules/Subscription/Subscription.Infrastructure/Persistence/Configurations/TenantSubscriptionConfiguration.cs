using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscription.Domain.Entities;

namespace Subscription.Infrastructure.Persistence.Configurations;

public sealed class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("TenantSubscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.PlanCode).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.StartedAtUtc).IsRequired();
        builder.Property(x => x.CancelledAtUtc);
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.HasIndex(x => x.TenantId).IsUnique();
        builder.HasIndex(x => new { x.PlanCode, x.Status });
    }
}
