using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Domain.Entities;

namespace Shipment.Infrastructure.Persistence.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment.Domain.Entities.Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment.Domain.Entities.Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ShipmentNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RecipientName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RecipientPhoneNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CarrierCode).HasMaxLength(50);
        builder.Property(x => x.CarrierName).HasMaxLength(200);
        builder.Property(x => x.ServiceCode).HasMaxLength(50);
        builder.Property(x => x.ServiceName).HasMaxLength(200);
        builder.Property(x => x.TrackingUrl).HasMaxLength(500);
        builder.Property(x => x.InternalNote).HasMaxLength(1000);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.Property(x => x.Status).IsRequired();

        builder.OwnsOne(x => x.DestinationAddress, owned =>
        {
            owned.Property(x => x.ContactName).HasColumnName("DestinationContactName").HasMaxLength(200).IsRequired();
            owned.Property(x => x.PhoneNumber).HasColumnName("DestinationPhoneNumber").HasMaxLength(50).IsRequired();
            owned.Property(x => x.Country).HasColumnName("DestinationCountry").HasMaxLength(100).IsRequired();
            owned.Property(x => x.City).HasColumnName("DestinationCity").HasMaxLength(100).IsRequired();
            owned.Property(x => x.District).HasColumnName("DestinationDistrict").HasMaxLength(100).IsRequired();
            owned.Property(x => x.Line1).HasColumnName("DestinationLine1").HasMaxLength(250).IsRequired();
            owned.Property(x => x.Line2).HasColumnName("DestinationLine2").HasMaxLength(250);
            owned.Property(x => x.PostalCode).HasColumnName("DestinationPostalCode").HasMaxLength(30);
        });

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Packages)
            .WithOne()
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.StoreId, x.ShipmentNumber }).IsUnique();
        builder.HasIndex(x => new { x.StoreId, x.OrderId });
        builder.HasIndex(x => new { x.StoreId, x.Status, x.CreatedAtUtc });
    }
}
