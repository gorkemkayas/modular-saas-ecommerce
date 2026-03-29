namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontProductCategoryDto(
        Guid Id,
        string Name,
        string Slug,
        bool IsPrimary,
        int SortOrder);
}
