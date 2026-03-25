using Catalog.Domain.Common;
using Catalog.Domain.Enums;
using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities
{
    public sealed class Product : IAggregateRoot
    {
        private readonly List<ProductVariant> _variants = new();
        private readonly List<ProductCategory> _categories = new();
        private readonly List<ProductAttributeValue> _attributeValues = new();
        private readonly List<ProductMedia> _mediaItems = new();

        public Guid Id { get; private set; }
        public Guid StoreId { get; private set; }
        public string Name { get; private set; } = default!;
        public string? ShortDescription { get; private set; }
        public string? Description { get; private set; }
        public Slug Slug { get; private set; } = default!;
        public Guid? BrandId { get; private set; }

        // Simple product için kullanılır.
        // Variant product ise null olmalıdır.
        public Sku? Sku { get; private set; }

        public ProductType ProductType { get; private set; }
        public ProductStatus ProductStatus { get; private set; }
        public bool IsPublished { get; private set; }
        public DateTime? PublishedAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }

        public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
        public IReadOnlyCollection<ProductCategory> Categories => _categories.AsReadOnly();
        public IReadOnlyCollection<ProductAttributeValue> AttributeValues => _attributeValues.AsReadOnly();
        public IReadOnlyCollection<ProductMedia> MediaItems => _mediaItems.AsReadOnly();

        private Product()
        {
        }

        private Product(
            Guid id,
            Guid storeId,
            string name,
            string? shortDescription,
            string? description,
            Slug slug,
            Guid? brandId,
            ProductType productType,
            Sku? sku)
        {
            if (storeId == Guid.Empty)
                throw new ArgumentException("StoreId cannot be empty.", nameof(storeId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty.", nameof(name));

            if (productType == ProductType.Simple && sku is null)
                throw new CatalogDomainException("Simple product must have a SKU.");

            if (productType == ProductType.Variant && sku is not null)
                throw new CatalogDomainException("Variant product cannot have a product-level SKU.");

            Id = id;
            StoreId = storeId;
            Name = name.Trim();
            ShortDescription = string.IsNullOrWhiteSpace(shortDescription) ? null : shortDescription.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Slug = slug;
            BrandId = brandId;
            ProductType = productType;
            Sku = sku;
            ProductStatus = ProductStatus.Draft;
            IsPublished = false;
            PublishedAtUtc = null;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public static Product Create(
            Guid storeId,
            string name,
            Slug slug,
            ProductType productType,
            Sku? sku = null,
            string? shortDescription = null,
            string? description = null,
            Guid? brandId = null)
        {
            return new Product(
                Guid.NewGuid(),
                storeId,
                name,
                shortDescription,
                description,
                slug,
                brandId,
                productType,
                sku);
        }

        public static Product CreateSimple(
            Guid storeId,
            string name,
            Slug slug,
            Sku sku,
            string? shortDescription = null,
            string? description = null,
            Guid? brandId = null)
        {
            return new Product(
                Guid.NewGuid(),
                storeId,
                name,
                shortDescription,
                description,
                slug,
                brandId,
                ProductType.Simple,
                sku);
        }

        public static Product CreateVariant(
            Guid storeId,
            string name,
            Slug slug,
            string? shortDescription = null,
            string? description = null,
            Guid? brandId = null)
        {
            return new Product(
                Guid.NewGuid(),
                storeId,
                name,
                shortDescription,
                description,
                slug,
                brandId,
                ProductType.Variant,
                sku: null);
        }

        public void UpdateDetails(string name, string? shortDescription, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty.", nameof(name));

            Name = name.Trim();
            ShortDescription = string.IsNullOrWhiteSpace(shortDescription) ? null : shortDescription.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeSlug(Slug slug)
        {
            Slug = slug;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeBrand(Guid? brandId)
        {
            BrandId = brandId;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void SetSku(Sku sku)
        {
            if (ProductType != ProductType.Simple)
                throw new CatalogDomainException("Only simple products can have a product-level SKU.");

            Sku = sku;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ClearSku()
        {
            if (ProductType == ProductType.Simple)
                throw new CatalogDomainException("Simple product must have a SKU.");

            Sku = null;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ConvertToVariant()
        {
            if (IsPublished)
                throw new CatalogDomainException("Published product type cannot be changed directly.");

            if (ProductType == ProductType.Variant)
                return;

            // simple -> variant
            Sku = null;
            ProductType = ProductType.Variant;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ConvertToSimple(Sku sku)
        {
            if (IsPublished)
                throw new CatalogDomainException("Published product type cannot be changed directly.");

            if (ProductType == ProductType.Simple)
            {
                Sku = sku;
                UpdatedAtUtc = DateTime.UtcNow;
                return;
            }

            // variant -> simple
            RemoveAllVariantsAndTheirMedia();

            Sku = sku;
            ProductType = ProductType.Simple;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (ProductStatus == ProductStatus.Archived)
                throw new CatalogDomainException("Archived product cannot be activated.");

            ProductStatus = ProductStatus.Active;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Archive()
        {
            ProductStatus = ProductStatus.Archived;
            IsPublished = false;
            PublishedAtUtc = null;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void AddCategory(Guid categoryId, bool isPrimary = false, int sortOrder = 0)
        {
            if (categoryId == Guid.Empty)
                throw new ArgumentException("CategoryId cannot be empty.", nameof(categoryId));

            if (_categories.Any(x => x.CategoryId == categoryId))
                return;

            if (isPrimary)
            {
                foreach (var item in _categories)
                    item.SetPrimary(false);
            }

            _categories.Add(ProductCategory.Create(Id, categoryId, isPrimary, sortOrder));
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void AssignCategories(IEnumerable<Guid> categoryIds)
        {
            var distinctIds = categoryIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            _categories.Clear();

            for (int i = 0; i < distinctIds.Count; i++)
            {
                _categories.Add(ProductCategory.Create(
                    productId: Id,
                    categoryId: distinctIds[i],
                    isPrimary: i == 0,
                    sortOrder: i));
            }

            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void RemoveCategory(Guid categoryId)
        {
            var item = _categories.FirstOrDefault(x => x.CategoryId == categoryId);
            if (item is null)
                return;

            _categories.Remove(item);

            if (!_categories.Any(x => x.IsPrimary) && _categories.Count > 0)
                _categories[0].SetPrimary(true);

            UpdatedAtUtc = DateTime.UtcNow;
        }

        public ProductVariant AddVariant(
            Sku sku,
            string? name,
            int sortOrder,
            IReadOnlyCollection<(Guid attributeDefinitionId, string value)> variantAttributes)
        {
            if (ProductType != ProductType.Variant)
                throw new CatalogDomainException("Variants can only be added to variant products.");

            if (_variants.Any(x => x.Sku == sku))
                throw new DuplicateSkuException(sku.Value);

            var newSignature = BuildVariantSignature(variantAttributes);

            foreach (var existingVariant in _variants)
            {
                var existingSignature = BuildVariantSignature(
                    existingVariant.AttributeValues
                        .Select(x => (x.AttributeDefinitionId, x.Value))
                        .ToList());

                if (existingSignature == newSignature)
                    throw new DuplicateVariantCombinationException();
            }

            var variant = ProductVariant.Create(Id, sku, name, sortOrder);

            foreach (var attr in variantAttributes)
            {
                var attributeValue = ProductAttributeValue.CreateForVariant(
                    attributeDefinitionId: attr.attributeDefinitionId,
                    productVariantId: variant.Id,
                    value: attr.value);

                variant.AddAttributeValue(attributeValue);
            }

            _variants.Add(variant);
            UpdatedAtUtc = DateTime.UtcNow;

            return variant;
        }

        public void RemoveVariant(Guid variantId)
        {
            var variant = _variants.FirstOrDefault(x => x.Id == variantId);
            if (variant is null)
                return;

            _mediaItems.RemoveAll(x => x.ProductVariantId == variantId);
            _variants.Remove(variant);
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void SetProductAttributeValue(Guid attributeDefinitionId, string value)
        {
            var existing = _attributeValues
                .FirstOrDefault(x => x.AttributeDefinitionId == attributeDefinitionId && x.ProductId == Id);

            if (existing is null)
            {
                _attributeValues.Add(ProductAttributeValue.CreateForProduct(attributeDefinitionId, Id, value));
            }
            else
            {
                existing.ChangeValue(value);
            }

            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void AddMedia(
            MediaType mediaType,
            string url,
            string? altText,
            bool isMain,
            int sortOrder,
            Guid? productVariantId = null)
        {
            if (productVariantId.HasValue && !_variants.Any(x => x.Id == productVariantId.Value))
                throw new CatalogDomainException("Media can only be linked to a variant that belongs to this product.");

            if (isMain)
            {
                foreach (var media in _mediaItems.Where(x => x.ProductVariantId == productVariantId))
                    media.UnmarkAsMain();
            }

            var mediaItem = ProductMedia.Create(
                productId: Id,
                mediaType: mediaType,
                url: url,
                altText: altText,
                isMain: isMain,
                sortOrder: sortOrder,
                productVariantId: productVariantId);

            _mediaItems.Add(mediaItem);
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Publish()
        {
            if (ProductStatus == ProductStatus.Archived)
                throw new InvalidProductPublishStateException("Archived product cannot be published.");

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidProductPublishStateException("Product name is required.");

            if (Slug is null)
                throw new InvalidProductPublishStateException("Product slug is required.");

            if (!_categories.Any())
                throw new InvalidProductPublishStateException("Product must have at least one category.");

            if (ProductType == ProductType.Simple)
            {
                if (Sku is null)
                    throw new InvalidProductPublishStateException("Simple product must have a SKU.");
            }

            if (ProductType == ProductType.Variant)
            {
                if (_variants.Count == 0)
                    throw new InvalidProductPublishStateException("Variant product must have at least one variant.");

                if (_variants.Any(x => x.Sku is null))
                    throw new InvalidProductPublishStateException("All variants must have a SKU.");
            }

            ProductStatus = ProductStatus.Active;
            IsPublished = true;
            PublishedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Unpublish()
        {
            if (!IsPublished)
                return;

            IsPublished = false;
            PublishedAtUtc = null;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        private static string BuildVariantSignature(IReadOnlyCollection<(Guid attributeDefinitionId, string value)> attributes)
        {
            return string.Join(
                "|",
                attributes
                    .Where(x => x.attributeDefinitionId != Guid.Empty)
                    .Select(x => $"{x.attributeDefinitionId:N}={x.value.Trim().ToLowerInvariant()}")
                    .OrderBy(x => x));
        }
        private void RemoveAllVariantsAndTheirMedia()
        {
            if (_variants.Count == 0)
                return;

            var variantIds = _variants.Select(x => x.Id).ToHashSet();

            _mediaItems.RemoveAll(x => x.ProductVariantId.HasValue && variantIds.Contains(x.ProductVariantId.Value));
            _variants.Clear();
        }
    }
}
