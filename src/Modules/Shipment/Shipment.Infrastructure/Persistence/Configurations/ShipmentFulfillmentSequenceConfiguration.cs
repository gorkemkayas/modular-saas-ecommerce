using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Infrastructure.Persistence.Fulfillment;

namespace Shipment.Infrastructure.Persistence.Configurations;

public sealed class ShipmentFulfillmentSequenceConfiguration
    : IEntityTypeConfiguration<ShipmentFulfillmentSequence>
{
    public void Configure(EntityTypeBuilder<ShipmentFulfillmentSequence> builder)
    {
        builder.ToTable("ShipmentFulfillmentSequences");
        builder.HasKey(x => new { x.StoreId, x.OrderId });
        builder.Property(x => x.StoreId).ValueGeneratedNever();
        builder.Property(x => x.OrderId).ValueGeneratedNever();
        builder.Property(x => x.LastRevision).IsRequired();
    }
}
