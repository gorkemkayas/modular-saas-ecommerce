using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Domain.Entities;

namespace Shipment.Infrastructure.Persistence.Configurations;

public sealed class ShipmentLineConfiguration : IEntityTypeConfiguration<ShipmentLine>
{
    public void Configure(EntityTypeBuilder<ShipmentLine> builder)
    {
        builder.ToTable("ShipmentLines");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.VariantName).HasMaxLength(200);
        builder.Property(x => x.Sku).HasMaxLength(100);
        builder.Property(x => x.Quantity).IsRequired();

        builder.HasIndex(x => x.ShipmentId);
    }
}
