namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontCategoryTreeNodeDto(
        Guid Id,
        string Name,
        string Slug,
        string? Description,
        string? ImageUrl,
        Guid? ParentCategoryId,
        int SortOrder,
        IReadOnlyCollection<StorefrontCategoryTreeNodeDto> Children);
}
