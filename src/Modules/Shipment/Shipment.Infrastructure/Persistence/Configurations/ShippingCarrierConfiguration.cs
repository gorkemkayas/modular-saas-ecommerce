using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Domain.Entities;

namespace Shipment.Infrastructure.Persistence.Configurations;

public sealed class ShippingCarrierConfiguration : IEntityTypeConfiguration<ShippingCarrier>
{
    public void Configure(EntityTypeBuilder<ShippingCarrier> builder)
    {
        builder.ToTable("ShippingCarriers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.StoreId).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ServiceCode).HasMaxLength(50);
        builder.Property(x => x.ServiceName).HasMaxLength(200);
        builder.Property(x => x.TrackingUrl).HasMaxLength(500);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.StoreId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.StoreId, x.IsActive, x.SortOrder });
    }
}
