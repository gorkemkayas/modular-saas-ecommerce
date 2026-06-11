using Store.Domain.Abstractions;
using Store.Domain.Exceptions;
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
        public string? HeroImageUrl { get; private set; }
        public StorefrontHeroMediaType? HeroMediaType { get; private set; }
        public string? HeroEyebrowText { get; private set; }
        public string? HeroTitle { get; private set; }
        public string? HeroAccentTitle { get; private set; }
        public string? HeroDescription { get; private set; }
        public string? HeroPrimaryButtonText { get; private set; }
        public string? LoginPageImageUrl { get; private set; }
        public string? RegisterPageImageUrl { get; private set; }
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
            string? logoUrl,
            string? heroImageUrl,
            StorefrontHeroMediaType? heroMediaType,
            string? heroEyebrowText,
            string? heroTitle,
            string? heroAccentTitle,
            string? heroDescription,
            string? heroPrimaryButtonText,
            string? loginPageImageUrl,
            string? registerPageImageUrl)
        {
            if (tenantId == Guid.Empty)
                throw new InvalidTenantException();

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidStoreNameException();

            Id = id;
            TenantId = tenantId;
            Name = name.Trim();
            Slug = slug;
            Description = description?.Trim();
            LogoUrl = logoUrl?.Trim();
            HeroImageUrl = heroImageUrl?.Trim();
            HeroMediaType = heroMediaType;
            HeroEyebrowText = heroEyebrowText?.Trim();
            HeroTitle = heroTitle?.Trim();
            HeroAccentTitle = heroAccentTitle?.Trim();
            HeroDescription = heroDescription?.Trim();
            HeroPrimaryButtonText = heroPrimaryButtonText?.Trim();
            LoginPageImageUrl = loginPageImageUrl?.Trim();
            RegisterPageImageUrl = registerPageImageUrl?.Trim();
            Status = StoreStatus.Active;
            IsPublished = false;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public static Store Create(
            Guid tenantId,
            string name,
            Slug slug,
            string? description = null,
            string? logoUrl = null,
            string? heroImageUrl = null,
            StorefrontHeroMediaType? heroMediaType = null,
            string? heroEyebrowText = null,
            string? heroTitle = null,
            string? heroAccentTitle = null,
            string? heroDescription = null,
            string? heroPrimaryButtonText = null,
            string? loginPageImageUrl = null,
            string? registerPageImageUrl = null)
        {
            return new Store(
                Guid.NewGuid(),
                tenantId,
                name,
                slug,
                description,
                logoUrl,
                heroImageUrl,
                heroMediaType,
                heroEyebrowText,
                heroTitle,
                heroAccentTitle,
                heroDescription,
                heroPrimaryButtonText,
                loginPageImageUrl,
                registerPageImageUrl);
        }

        public void UpdateProfile(
            string name,
            string? description,
            string? logoUrl,
            string? heroImageUrl = null,
            StorefrontHeroMediaType? heroMediaType = null,
            string? heroEyebrowText = null,
            string? heroTitle = null,
            string? heroAccentTitle = null,
            string? heroDescription = null,
            string? heroPrimaryButtonText = null,
            string? loginPageImageUrl = null,
            string? registerPageImageUrl = null)
        {
            EnsureNotArchived();

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidStoreNameException();

            Name = name.Trim();
            Description = description?.Trim();
            LogoUrl = logoUrl?.Trim();
            HeroImageUrl = heroImageUrl?.Trim();
            HeroMediaType = heroMediaType;
            HeroEyebrowText = heroEyebrowText?.Trim();
            HeroTitle = heroTitle?.Trim();
            HeroAccentTitle = heroAccentTitle?.Trim();
            HeroDescription = heroDescription?.Trim();
            HeroPrimaryButtonText = heroPrimaryButtonText?.Trim();
            LoginPageImageUrl = loginPageImageUrl?.Trim();
            RegisterPageImageUrl = registerPageImageUrl?.Trim();
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeSlug(Slug newSlug)
        {
            EnsureNotArchived();

            if (Slug == newSlug)
                throw new DuplicateSlugException();

            Slug = newSlug;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Publish()
        {
            EnsureNotArchived();

            if (Status != StoreStatus.Active)
                throw InvalidStoreStatusException.CannotPublish();

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
                throw InvalidStoreStatusException.CannotActivate();

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
                throw new ArchivedStoreException();
        }
    }
}
