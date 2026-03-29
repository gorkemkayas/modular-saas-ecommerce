namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontCatalogFacetCriteria(
        Guid StoreId,
        string? SearchTerm,
        Guid? CategoryId,
        Guid? BrandId);
}
