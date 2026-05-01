using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Domain.Entities;

namespace Shipment.Infrastructure.Persistence.Configurations;

public sealed class TrackingEventConfiguration : IEntityTypeConfiguration<TrackingEvent>
{
    public void Configure(EntityTypeBuilder<TrackingEvent> builder)
    {
        builder.ToTable("TrackingEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Location).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.RawStatusCode).HasMaxLength(100);
        builder.Property(x => x.RawStatusText).HasMaxLength(500);

        builder.HasIndex(x => new { x.ShipmentPackageId, x.OccurredAtUtc });
    }
}
