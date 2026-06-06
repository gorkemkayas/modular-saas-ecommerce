using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Persistence.Configurations;

public sealed class PaymentProviderAccountConfiguration : IEntityTypeConfiguration<PaymentProviderAccount>
{
    public void Configure(EntityTypeBuilder<PaymentProviderAccount> builder)
    {
        builder.ToTable("PaymentProviderAccounts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.StoreId).IsRequired();
        builder.Property(x => x.Provider).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.ApiKeyCipherText).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.SecretKeyCipherText).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ApiKeyLastFour).HasMaxLength(8).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.HasIndex(x => new { x.StoreId, x.Provider }).IsUnique();
        builder.HasIndex(x => new { x.StoreId, x.Status });
    }
}
