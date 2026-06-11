using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment.Domain.Entities.Payment>
{
    public void Configure(EntityTypeBuilder<Payment.Domain.Entities.Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.StoreId).IsRequired();
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Provider).IsRequired();
        builder.Property(x => x.ProviderAccountId);
        builder.Property(x => x.MethodType).IsRequired();
        builder.Property(x => x.ExternalPaymentReference).HasMaxLength(200);
        builder.Property(x => x.ExternalConversationId).HasMaxLength(200);
        builder.Property(x => x.FailureCode).HasMaxLength(100);
        builder.Property(x => x.FailureMessage).HasMaxLength(500);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Refunds)
            .WithOne()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PaymentProviderAccount>()
            .WithMany()
            .HasForeignKey(x => x.ProviderAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.StoreId, x.OrderId }).IsUnique();
        builder.HasIndex(x => new { x.StoreId, x.Status, x.CreatedAtUtc });
        builder.HasIndex(x => x.ProviderAccountId);
        builder.HasIndex(x => x.ExternalPaymentReference);
        builder.HasIndex(x => x.ExternalConversationId);
    }
}
