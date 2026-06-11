namespace ECommerce.API.Contracts.Store.UpdateStoreProfile
{
    public sealed record UpdateStoreProfileRequest(
        string Name,
        string? Description,
        string? LogoUrl,
        string? HeroImageUrl = null,
        string? HeroMediaType = null,
        string? HeroEyebrowText = null,
        string? HeroTitle = null,
        string? HeroAccentTitle = null,
        string? HeroDescription = null,
        string? HeroPrimaryButtonText = null,
        string? LoginPageImageUrl = null,
        string? RegisterPageImageUrl = null);
}
