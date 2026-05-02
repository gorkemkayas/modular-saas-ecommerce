namespace ECommerce.API.Contracts.Catalog.Storefront
{
    public sealed record StorefrontProductSearchRequest(
        string? SearchTerm,
        Guid? CategoryId,
        Guid? BrandId,
        string CurrencyCode = "TRY",
        int PageNumber = 1,
        int PageSize = 20);

    public sealed record StorefrontBrandSearchRequest(string? SearchTerm);

    public sealed record StorefrontCatalogFacetsRequest(
        string? SearchTerm,
        Guid? CategoryId,
        Guid? BrandId);
}
