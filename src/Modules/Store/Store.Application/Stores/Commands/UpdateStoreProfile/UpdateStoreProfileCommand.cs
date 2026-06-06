using MediatR;
using Store.Domain.Stores;

namespace Store.Application.Stores.Commands.UpdateStoreProfile
{
    public sealed record UpdateStoreProfileCommand(
        Guid TenantId,
        string Name,
        string? Description,
        string? LogoUrl,
        string? HeroImageUrl = null,
        StorefrontHeroMediaType? HeroMediaType = null,
        string? HeroEyebrowText = null,
        string? HeroTitle = null,
        string? HeroAccentTitle = null,
        string? HeroDescription = null,
        string? HeroPrimaryButtonText = null,
        string? LoginPageImageUrl = null,
        string? RegisterPageImageUrl = null) : IRequest;
}
