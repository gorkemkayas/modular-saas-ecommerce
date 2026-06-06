using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public sealed class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
    {
        public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
        {
            builder.ToTable("AttributeDefinitions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.StoreId)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Code)
                .HasConversion(
                    code => code.Value,
                    value => AttributeCode.Create(value))
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.DataType)
                .IsRequired();

            builder.Property(x => x.IsRequired)
                .IsRequired();

            builder.Property(x => x.IsFilterable)
                .IsRequired();

            builder.Property(x => x.IsVariantDefining)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => new { x.StoreId, x.Code })
                .IsUnique();

            builder.HasIndex(x => new { x.StoreId, x.IsActive });
        }
    }
}
