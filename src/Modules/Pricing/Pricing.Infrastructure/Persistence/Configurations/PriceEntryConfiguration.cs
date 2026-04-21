using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Domain.Entities;

namespace Pricing.Infrastructure.Persistence.Configurations;

public sealed class PriceEntryConfiguration : IEntityTypeConfiguration<PriceEntry>
{
    public void Configure(EntityTypeBuilder<PriceEntry> builder)
    {
        builder.ToTable("PriceEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriceListId)
            .IsRequired();

        builder.OwnsOne(x => x.Target, target =>
        {
            target.Property(x => x.ProductId)
                .HasColumnName("ProductId")
                .IsRequired();

            target.Property(x => x.ProductVariantId)
                .HasColumnName("ProductVariantId");
        });

        builder.OwnsOne(x => x.Price, price =>
        {
            price.Property(x => x.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            price.OwnsOne(x => x.Currency, currency =>
            {
                currency.Property(x => x.Code)
                    .HasColumnName("CurrencyCode")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        builder.OwnsOne(x => x.CompareAtPrice, compareAt =>
        {
            compareAt.Property(x => x.Amount)
                .HasColumnName("CompareAtAmount")
                .HasPrecision(18, 2);

            compareAt.OwnsOne(x => x.Currency, currency =>
            {
                currency.Property(x => x.Code)
                    .HasColumnName("CompareAtCurrencyCode")
                    .HasMaxLength(3);
            });
        });

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.PriceListId, x.IsActive });
    }
}
