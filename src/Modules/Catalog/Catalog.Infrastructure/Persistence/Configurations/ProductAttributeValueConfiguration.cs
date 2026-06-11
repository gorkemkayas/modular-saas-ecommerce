using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public sealed class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
    {
        public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
        {
            builder.ToTable("ProductAttributeValues", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_ProductAttributeValues_Scope",
                    "(\"ProductId\" IS NOT NULL AND \"ProductVariantId\" IS NULL) OR (\"ProductId\" IS NULL AND \"ProductVariantId\" IS NOT NULL)");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.AttributeDefinitionId)
                .IsRequired();

            builder.Property(x => x.Value)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => new { x.ProductId, x.AttributeDefinitionId })
                .IsUnique()
                .HasFilter("\"ProductId\" IS NOT NULL");

            builder.HasIndex(x => new { x.ProductVariantId, x.AttributeDefinitionId })
                .IsUnique()
                .HasFilter("\"ProductVariantId\" IS NOT NULL");

            builder.HasOne<AttributeDefinition>()
                .WithMany()
                .HasForeignKey(x => x.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Product>()
                .WithMany(x => x.AttributeValues)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ProductVariant>()
                .WithMany(x => x.AttributeValues)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
