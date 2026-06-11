using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscription.Domain.Entities;

namespace Subscription.Infrastructure.Persistence.Configurations;

public sealed class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.ToTable("PlanFeatures");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanId).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(120).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(300);

        builder.HasIndex(x => new { x.PlanId, x.Key }).IsUnique();
    }
}
