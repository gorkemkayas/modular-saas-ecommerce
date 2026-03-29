namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontProductSearchCriteria(
        Guid StoreId,
        string? SearchTerm,
        Guid? CategoryId,
        Guid? BrandId,
        int PageNumber = 1,
        int PageSize = 20);
}
