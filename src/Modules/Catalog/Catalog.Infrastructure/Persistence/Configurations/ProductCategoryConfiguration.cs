using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("ProductCategories");

            builder.HasKey(x => new { x.ProductId, x.CategoryId });

            builder.Property(x => x.IsPrimary)
                .IsRequired();

            builder.Property(x => x.SortOrder)
                .IsRequired();

            builder.HasIndex(x => new { x.ProductId, x.SortOrder });

            builder.HasOne<Product>()
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
