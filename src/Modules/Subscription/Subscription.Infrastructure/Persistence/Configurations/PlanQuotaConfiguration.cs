using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscription.Domain.Entities;

namespace Subscription.Infrastructure.Persistence.Configurations;

public sealed class PlanQuotaConfiguration : IEntityTypeConfiguration<PlanQuota>
{
    public void Configure(EntityTypeBuilder<PlanQuota> builder)
    {
        builder.ToTable("PlanQuotas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanId).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Limit);

        builder.HasIndex(x => new { x.PlanId, x.Key }).IsUnique();
    }
}
