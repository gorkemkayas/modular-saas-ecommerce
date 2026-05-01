using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationDispatchConfiguration : IEntityTypeConfiguration<NotificationDispatch>
{
    public void Configure(EntityTypeBuilder<NotificationDispatch> builder)
    {
        builder.ToTable("NotificationDispatches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Channel).IsRequired();
        builder.Property(x => x.Trigger).IsRequired();
        builder.Property(x => x.RecipientAddress).HasMaxLength(320);
        builder.Property(x => x.RecipientName).HasMaxLength(200);
        builder.Property(x => x.Subject).HasMaxLength(500);
        builder.Property(x => x.Body).HasMaxLength(20000);
        builder.Property(x => x.BusinessEntityType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProviderName).HasMaxLength(100);
        builder.Property(x => x.ProviderMessageId).HasMaxLength(200);
        builder.Property(x => x.FailureCode).HasMaxLength(100);
        builder.Property(x => x.FailureMessage).HasMaxLength(1000);
        builder.Property(x => x.SuppressionReason).HasMaxLength(500);
        builder.Property(x => x.LastProviderEventType).HasMaxLength(100);

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey(x => x.NotificationDispatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.StoreId,
            x.Trigger,
            x.Channel,
            x.BusinessEntityType,
            x.BusinessEntityId
        }).IsUnique();

        builder.HasIndex(x => new { x.StoreId, x.Status, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.ProviderName, x.ProviderMessageId });
    }
}
