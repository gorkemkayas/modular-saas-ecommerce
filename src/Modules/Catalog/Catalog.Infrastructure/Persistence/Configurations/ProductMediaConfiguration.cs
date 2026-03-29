using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public sealed class ProductMediaConfiguration : IEntityTypeConfiguration<ProductMedia>
    {
        public void Configure(EntityTypeBuilder<ProductMedia> builder)
        {
            builder.ToTable("ProductMedia");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductId)
                .IsRequired();

            builder.Property(x => x.MediaType)
                .IsRequired();

            builder.Property(x => x.Url)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.AltText)
                .HasMaxLength(500);

            builder.Property(x => x.IsMain)
                .IsRequired();

            builder.Property(x => x.SortOrder)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => new { x.ProductId, x.ProductVariantId, x.SortOrder });

            builder.HasOne<Product>()
                .WithMany(x => x.MediaItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ProductVariant>()
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
