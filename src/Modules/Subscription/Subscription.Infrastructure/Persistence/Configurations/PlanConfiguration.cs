using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscription.Domain.Entities;

namespace Subscription.Infrastructure.Persistence.Configurations;

public sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.MonthlyPriceAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsPublic).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.ExternalPricingPlanReferenceCode).HasMaxLength(500);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.IsPublic, x.IsActive, x.DisplayOrder });

        builder.HasMany(x => x.Features)
            .WithOne()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Quotas)
            .WithOne()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Features).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Quotas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
