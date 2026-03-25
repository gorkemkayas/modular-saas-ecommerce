using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities
{
    public sealed class Brand : IAggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid StoreId { get; private set; }
        public string Name { get; private set; } = default!;
        public Slug Slug { get; private set; } = default!;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }

        private Brand()
        {
        }

        private Brand(
            Guid id,
            Guid storeId,
            string name,
            Slug slug,
            string? description)
        {
            if (storeId == Guid.Empty)
                throw new ArgumentException("StoreId cannot be empty.", nameof(storeId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Brand name cannot be empty.", nameof(name));

            Id = id;
            StoreId = storeId;
            Name = name.Trim();
            Slug = slug;
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public static Brand Create(Guid storeId, string name, Slug slug, string? description = null)
        {
            return new Brand(Guid.NewGuid(), storeId, name, slug, description);
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Brand name cannot be empty.", nameof(name));

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
