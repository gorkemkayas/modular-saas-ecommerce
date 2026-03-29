namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontProductVariantDto(
        Guid Id,
        string? Name,
        IReadOnlyCollection<StorefrontProductAttributeDto> Attributes,
        IReadOnlyCollection<StorefrontProductMediaDto> MediaItems);
}
