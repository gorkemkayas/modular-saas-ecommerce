using Store.Domain.Stores;

namespace Store.Application.DTOs
{
    public sealed record StoreDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    StoreStatus Status,
    bool IsPublished,
    string? HeroImageUrl = null,
    StorefrontHeroMediaType? HeroMediaType = null,
    string? HeroEyebrowText = null,
    string? HeroTitle = null,
    string? HeroAccentTitle = null,
    string? HeroDescription = null,
    string? HeroPrimaryButtonText = null,
    string? LoginPageImageUrl = null,
    string? RegisterPageImageUrl = null);
}
