namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontProductVariantDto(
        Guid Id,
        string? Name,
        StorefrontResolvedPriceDto? Price,
        IReadOnlyCollection<StorefrontProductAttributeDto> Attributes,
        IReadOnlyCollection<StorefrontProductMediaDto> MediaItems);
}
