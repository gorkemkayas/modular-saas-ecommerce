using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Infrastructure.Persistence.Inbox;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class InboxMessageConfiguration
    : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

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

        builder.Property(x => x.ReceivedOnUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.AttemptCount)
            .IsRequired();

        builder.Property(x => x.NextAttemptAtUtc)
            .IsRequired();

        builder.Property(x => x.LockId);

        builder.Property(x => x.LockedUntilUtc);

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        builder.Property(x => x.ProcessedOnUtc);

        builder.HasIndex(x => new
        {
            x.NextAttemptAtUtc,
            x.ReceivedOnUtc
        })
        .HasFilter(
            $"\"Status\" = {(int)InboxMessageStatus.Pending}");

        builder.HasIndex(x => new
        {
            x.LockedUntilUtc,
            x.ReceivedOnUtc
        })
        .HasFilter(
            $"\"Status\" = {(int)InboxMessageStatus.Processing}");

        builder.HasIndex(x => x.ProcessedOnUtc)
            .HasFilter(
                $"\"Status\" = {(int)InboxMessageStatus.Processed}");
    }
}