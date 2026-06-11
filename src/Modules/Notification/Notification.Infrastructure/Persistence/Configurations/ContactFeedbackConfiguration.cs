using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class ContactFeedbackConfiguration : IEntityTypeConfiguration<ContactFeedback>
{
    public void Configure(EntityTypeBuilder<ContactFeedback> builder)
    {
        builder.ToTable("ContactFeedbacks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(100);

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => new { x.Email, x.CreatedAtUtc });
    }
}
