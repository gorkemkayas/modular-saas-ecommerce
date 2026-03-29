using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StoreId)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.ShortDescription)
                .HasMaxLength(500);

            builder.Property(x => x.Description)
                .HasMaxLength(4000);

            builder.Property(x => x.Slug)
                .HasConversion(
                    slug => slug.Value,
                    value => Slug.Create(value))
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.BrandId);

            builder.Property(x => x.Sku)
                .HasConversion(
                    sku => sku == null ? null : sku.Value,
                    value => string.IsNullOrWhiteSpace(value) ? null : Sku.Create(value))
                .HasMaxLength(100);

            builder.Property(x => x.ProductType)
                .IsRequired();

            builder.Property(x => x.ProductStatus)
                .IsRequired();

            builder.Property(x => x.IsPublished)
                .IsRequired();

            builder.Property(x => x.PublishedAtUtc);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => new { x.StoreId, x.Slug })
                .IsUnique();

            builder.HasIndex(x => new { x.StoreId, x.Sku })
                .IsUnique()
                .HasFilter("\"Sku\" IS NOT NULL");

            builder.HasIndex(x => new { x.StoreId, x.ProductStatus, x.IsPublished });

            builder.HasMany(x => x.Variants)
                .WithOne()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Categories)
                .WithOne()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.AttributeValues)
                .WithOne()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.MediaItems)
                .WithOne()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Variants)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(x => x.Categories)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(x => x.AttributeValues)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(x => x.MediaItems)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
