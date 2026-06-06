using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Domain.ValueObjects;

namespace Store.Infrastructure.Persistance.Configurations
{
    public sealed class StoreConfiguration : IEntityTypeConfiguration<Store.Domain.Stores.Store>
    {
        public void Configure(EntityTypeBuilder<Store.Domain.Stores.Store> builder)
        {
            builder.ToTable("Stores");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenantId)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Slug)
                .HasConversion(
                    slug => slug.Value,
                    value => Slug.Create(value))
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.LogoUrl)
                .HasMaxLength(500);

            builder.Property(x => x.HeroImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.HeroMediaType);

            builder.Property(x => x.HeroEyebrowText)
                .HasMaxLength(120);

            builder.Property(x => x.HeroTitle)
                .HasMaxLength(120);

            builder.Property(x => x.HeroAccentTitle)
                .HasMaxLength(120);

            builder.Property(x => x.HeroDescription)
                .HasMaxLength(1000);

            builder.Property(x => x.HeroPrimaryButtonText)
                .HasMaxLength(80);

            builder.Property(x => x.LoginPageImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.RegisterPageImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.IsPublished)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc);

            builder.HasIndex(x => x.TenantId)
                .IsUnique();

            builder.HasIndex(x => x.Slug)
                .IsUnique();
        }
    }
}
