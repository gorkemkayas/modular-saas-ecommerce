using Store.Domain.Abstractions;
using Store.Domain.ValueObjects;

namespace Store.Domain.Stores
{
    public sealed class Store : IAggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public string Name { get; private set; } = default!;
        public Slug Slug { get; private set; } = default!;
        public string? Description { get; private set; }
        public string? LogoUrl { get; private set; }
        public StoreStatus Status { get; private set; }
        public bool IsPublished { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        private Store()
        {
        }

        private Store(
            Guid id,
            Guid tenantId,
            string name,
            Slug slug,
            string? description,
            string? logoUrl)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("TenantId cannot be empty.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Store name cannot be empty.");

            Id = id;
            TenantId = tenantId;
            Name = name.Trim();
            Slug = slug;
            Description = description?.Trim();
            LogoUrl = logoUrl?.Trim();
            Status = StoreStatus.Active;
            IsPublished = false;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public static Store Create(
            Guid tenantId,
            string name,
            Slug slug,
            string? description = null,
            string? logoUrl = null)
        {
            return new Store(
                Guid.NewGuid(),
                tenantId,
                name,
                slug,
                description,
                logoUrl);
        }

        public void UpdateProfile(string name, string? description, string? logoUrl)
        {
            EnsureNotArchived();

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Store name cannot be empty.");

            Name = name.Trim();
            Description = description?.Trim();
            LogoUrl = logoUrl?.Trim();
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeSlug(Slug newSlug)
        {
            EnsureNotArchived();

            if (Slug == newSlug)
                throw new InvalidOperationException("New slug cannot be the same as current slug.");

            Slug = newSlug;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Publish()
        {
            EnsureNotArchived();

            if (Status != StoreStatus.Active)
                throw new InvalidOperationException("Only active stores can be published.");

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Store name is required.");

            IsPublished = true;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Unpublish()
        {
            EnsureNotArchived();

            IsPublished = false;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Suspend()
        {
            EnsureNotArchived();

            Status = StoreStatus.Suspended;
            IsPublished = false;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (Status == StoreStatus.Archived)
                throw new InvalidOperationException("Archived store cannot be activated.");

            Status = StoreStatus.Active;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Archive()
        {
            Status = StoreStatus.Archived;
            IsPublished = false;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        private void EnsureNotArchived()
        {
            if (Status == StoreStatus.Archived)
                throw new InvalidOperationException("Archived store cannot be modified.");
        }
    }
}
