using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Locale).HasMaxLength(20).IsRequired();
        builder.Property(x => x.SubjectTemplate).HasMaxLength(500).IsRequired();
        builder.Property(x => x.BodyTemplate).HasMaxLength(12000).IsRequired();
        builder.Property(x => x.Trigger).IsRequired();
        builder.Property(x => x.Channel).IsRequired();

        builder.HasIndex(x => new { x.StoreId, x.Trigger, x.Channel, x.Locale }).IsUnique();
        builder.HasIndex(x => new { x.StoreId, x.IsActive, x.UpdatedAtUtc });
    }
}
