using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Sku).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.SellableItemKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OnHandQuantity).IsRequired();
        builder.Property(x => x.ReservedQuantity).IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.StoreId, x.SellableItemKey }).IsUnique();
        builder.HasIndex(x => new { x.StoreId, x.ProductId, x.ProductVariantId });
        builder.HasIndex(x => new { x.StoreId, x.ReorderThreshold });

        builder.HasMany(x => x.Reservations)
            .WithOne()
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Movements)
            .WithOne()
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Reservations).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Movements).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
