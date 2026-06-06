using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Domain.Entities;

namespace Shipment.Infrastructure.Persistence.Configurations;

public sealed class ShipmentPackageConfiguration : IEntityTypeConfiguration<ShipmentPackage>
{
    public void Configure(EntityTypeBuilder<ShipmentPackage> builder)
    {
        builder.ToTable("ShipmentPackages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.PackageNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TrackingNumber).HasMaxLength(120);
        builder.Property(x => x.Weight).HasPrecision(18, 3);
        builder.Property(x => x.WeightUnit).HasMaxLength(20);
        builder.Property(x => x.LabelReference).HasMaxLength(200);

        builder.HasMany(x => x.TrackingEvents)
            .WithOne()
            .HasForeignKey(x => x.ShipmentPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ShipmentId, x.PackageNumber }).IsUnique();
        builder.HasIndex(x => x.TrackingNumber);
    }
}
