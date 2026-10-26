using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence.Dispatch;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationDispatchDeliveryConfiguration
    : IEntityTypeConfiguration<NotificationDispatchDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDispatchDelivery> builder)
    {
        builder.ToTable("NotificationDispatchDeliveries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProviderName).HasMaxLength(100);
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.HasOne<NotificationDispatch>()
            .WithOne()
            .HasForeignKey<NotificationDispatchDelivery>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.Status, x.LockedUntilUtc });
    }
}
