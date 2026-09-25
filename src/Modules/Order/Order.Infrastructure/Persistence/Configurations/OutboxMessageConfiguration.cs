using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.StoreId)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.RoutingKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredOnUtc)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.PublishedAtUtc);

        builder.HasIndex(x => new
        {
            x.PublishedAtUtc,
            x.CreatedAtUtc
        });

        builder.HasIndex(x => new
        {
            x.StoreId,
            x.CreatedAtUtc
        });
    }
}