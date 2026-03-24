using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entitites
{
    public sealed class ProductVariant
    {
        private readonly List<ProductAttributeValue> _attributeValues = new();

        public Guid Id { get; private set; }
        public Guid ProductId { get; private set; }
        public Sku Sku { get; private set; } = default!;
        public string? Name { get; private set; }
        public bool IsActive { get; private set; }
        public int SortOrder { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }

        public IReadOnlyCollection<ProductAttributeValue> AttributeValues => _attributeValues.AsReadOnly();

        private ProductVariant()
        {
        }

        private ProductVariant(
            Guid id,
            Guid productId,
            Sku sku,
            string? name,
            int sortOrder)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ProductId cannot be empty.", nameof(productId));

            Id = id;
            ProductId = productId;
            Sku = sku;
            Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            SortOrder = sortOrder;
            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public static ProductVariant Create(Guid productId, Sku sku, string? name = null, int sortOrder = 0)
        {
            return new ProductVariant(Guid.NewGuid(), productId, sku, name, sortOrder);
        }

        public void ChangeSku(Sku sku)
        {
            Sku = sku;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Rename(string? name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeSortOrder(int sortOrder)
        {
            SortOrder = sortOrder;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (IsActive)
                return;

            IsActive = true;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal void AddAttributeValue(ProductAttributeValue value)
        {
            _attributeValues.Add(value);
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal void RemoveAllAttributeValues()
        {
            _attributeValues.Clear();
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
