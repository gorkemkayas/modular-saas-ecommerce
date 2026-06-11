using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.VariantName).HasMaxLength(200);
        builder.Property(x => x.Sku).HasMaxLength(100);
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.LineSubtotalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.LineDiscountAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.LineTaxAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.LineTotalAmount).HasPrecision(18, 2).IsRequired();

        builder.OwnsOne(x => x.UnitPriceSnapshot, owned =>
        {
            owned.Property(x => x.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2).IsRequired();
            owned.Property(x => x.CurrencyCode).HasColumnName("UnitPriceCurrencyCode").HasMaxLength(3).IsRequired();
            owned.Property(x => x.CompareAtAmount).HasColumnName("UnitPriceCompareAtAmount").HasPrecision(18, 2);
            owned.Property(x => x.PriceListId).HasColumnName("UnitPricePriceListId").IsRequired();
            owned.Property(x => x.PriceEntryId).HasColumnName("UnitPricePriceEntryId").IsRequired();
        });

        builder.HasIndex(x => x.OrderId);
    }
}
