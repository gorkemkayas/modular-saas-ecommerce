using Catalog.Domain.Common;
using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entitites
{
    public sealed class Category : IAggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid StoreId { get; private set; }
        public string Name { get; private set; } = default!;
        public Slug Slug { get; private set; } = default!;
        public string? Description { get; private set; }
        public Guid? ParentCategoryId { get; private set; }
        public bool IsActive { get; private set; }
        public int SortOrder { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }

        private Category()
        {
        }

        private Category(
            Guid id,
            Guid storeId,
            string name,
            Slug slug,
            string? description,
            Guid? parentCategoryId,
            int sortOrder)
        {
            if (storeId == Guid.Empty)
                throw new ArgumentException("StoreId cannot be empty.", nameof(storeId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty.", nameof(name));

            if (parentCategoryId == Guid.Empty)
                throw new ArgumentException("ParentCategoryId cannot be empty guid.", nameof(parentCategoryId));

            Id = id;
            StoreId = storeId;
            Name = name.Trim();
            Slug = slug;
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            ParentCategoryId = parentCategoryId;
            SortOrder = sortOrder;
            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public static Category Create(
            Guid storeId,
            string name,
            Slug slug,
            string? description = null,
            Guid? parentCategoryId = null,
            int sortOrder = 0)
        {
            return new Category(Guid.NewGuid(), storeId, name, slug, description, parentCategoryId, sortOrder);
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty.", nameof(name));

            Name = name.Trim();
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeSlug(Slug slug)
        {
            Slug = slug;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeDescription(string? description)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeParent(Guid? parentCategoryId)
        {
            if (parentCategoryId == Id)
                throw new InvalidCategoryHierarchyException("A category cannot be its own parent.");

            ParentCategoryId = parentCategoryId;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void SetSortOrder(int sortOrder)
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
    }
}
