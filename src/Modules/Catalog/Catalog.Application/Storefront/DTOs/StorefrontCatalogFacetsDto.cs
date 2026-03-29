namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontCatalogFacetsDto(
        IReadOnlyCollection<StorefrontBrandFacetDto> Brands,
        IReadOnlyCollection<StorefrontAttributeFacetDto> Attributes);
}
