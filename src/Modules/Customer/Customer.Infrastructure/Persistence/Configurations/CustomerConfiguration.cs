using Customer.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customer.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Domain.Entities.Customer>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.ExternalUserId)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Create(value))
            .HasMaxLength(320)
            .IsRequired();

        builder.ComplexProperty(x => x.Name, nameBuilder =>
        {
            nameBuilder.Property(x => x.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            nameBuilder.Property(x => x.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(x => x.PhoneNumber)
            .HasConversion(
                phone => phone == null ? null : phone.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : PhoneNumber.Create(value))
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.PreferredLanguage)
            .HasMaxLength(10);

        builder.Property(x => x.PreferredCurrency)
            .HasMaxLength(3);

        builder.Property(x => x.RegisteredAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.ExternalUserId })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.Status });

        builder.HasMany(x => x.Addresses)
            .WithOne()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Consents)
            .WithOne()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Consents)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
