using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationAttemptConfiguration : IEntityTypeConfiguration<NotificationAttempt>
{
    public void Configure(EntityTypeBuilder<NotificationAttempt> builder)
    {
        builder.ToTable("NotificationAttempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ProviderRequestReference).HasMaxLength(200);
        builder.Property(x => x.ProviderMessageId).HasMaxLength(200);
        builder.Property(x => x.FailureCode).HasMaxLength(100);
        builder.Property(x => x.FailureMessage).HasMaxLength(1000);
        builder.Property(x => x.Status).IsRequired();

        builder.HasIndex(x => new { x.NotificationDispatchId, x.AttemptNumber }).IsUnique();
        builder.HasIndex(x => new { x.NotificationDispatchId, x.AttemptedAtUtc });
    }
}
