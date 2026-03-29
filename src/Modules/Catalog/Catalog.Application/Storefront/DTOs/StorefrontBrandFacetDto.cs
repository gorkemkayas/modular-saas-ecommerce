namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontBrandFacetDto(
        Guid BrandId,
        string Name,
        string Slug,
        int ProductCount);
}
