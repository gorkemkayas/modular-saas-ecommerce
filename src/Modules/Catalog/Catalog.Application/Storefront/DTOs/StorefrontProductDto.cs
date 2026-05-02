using Catalog.Domain.Enums;

namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontProductDto(
        Guid Id,
        string Name,
        string? ShortDescription,
        string? Description,
        string Slug,
        Guid? BrandId,
        string? BrandName,
        ProductType ProductType,
        DateTime? PublishedAtUtc,
        StorefrontResolvedPriceDto? Price,
        IReadOnlyCollection<StorefrontProductCategoryDto> Categories,
        IReadOnlyCollection<StorefrontProductAttributeDto> Attributes,
        IReadOnlyCollection<StorefrontProductVariantDto> Variants,
        IReadOnlyCollection<StorefrontProductMediaDto> MediaItems);
}
