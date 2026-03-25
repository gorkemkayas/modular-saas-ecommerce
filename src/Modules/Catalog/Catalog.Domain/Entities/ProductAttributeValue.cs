using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities
{
    public sealed class ProductAttributeValue
    {
        public Guid Id { get; private set; }
        public Guid AttributeDefinitionId { get; private set; }
        public Guid? ProductId { get; private set; }
        public Guid? ProductVariantId { get; private set; }
        public string Value { get; private set; } = default!;
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }

        private ProductAttributeValue()
        {
        }

        private ProductAttributeValue(
            Guid id,
            Guid attributeDefinitionId,
            Guid? productId,
            Guid? productVariantId,
            string value)
        {
            if (attributeDefinitionId == Guid.Empty)
                throw new ArgumentException("AttributeDefinitionId cannot be empty.", nameof(attributeDefinitionId));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Attribute value cannot be empty.", nameof(value));

            var hasProduct = productId.HasValue;
            var hasVariant = productVariantId.HasValue;

            if (hasProduct == hasVariant)
                throw new InvalidAttributeUsageException("Exactly one of ProductId or ProductVariantId must be set.");

            Id = id;
            AttributeDefinitionId = attributeDefinitionId;
            ProductId = productId;
            ProductVariantId = productVariantId;
            Value = value.Trim();
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public static ProductAttributeValue CreateForProduct(
            Guid attributeDefinitionId,
            Guid productId,
            string value)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ProductId cannot be empty.", nameof(productId));

            return new ProductAttributeValue(
                Guid.NewGuid(),
                attributeDefinitionId,
                productId,
                null,
                value);
        }

        public static ProductAttributeValue CreateForVariant(
            Guid attributeDefinitionId,
            Guid productVariantId,
            string value)
        {
            if (productVariantId == Guid.Empty)
                throw new ArgumentException("ProductVariantId cannot be empty.", nameof(productVariantId));

            return new ProductAttributeValue(
                Guid.NewGuid(),
                attributeDefinitionId,
                null,
                productVariantId,
                value);
        }

        public void ChangeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Attribute value cannot be empty.", nameof(value));

            Value = value.Trim();
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
