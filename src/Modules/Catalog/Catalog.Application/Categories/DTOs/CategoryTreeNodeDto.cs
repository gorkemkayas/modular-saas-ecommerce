namespace Catalog.Application.Categories.DTOs
{
    public sealed record CategoryTreeNodeDto(
        Guid Id,
        Guid StoreId,
        string Name,
        string Slug,
        string? Description,
        string? ImageUrl,
        Guid? ParentCategoryId,
        bool IsActive,
        int SortOrder,
        IReadOnlyCollection<CategoryTreeNodeDto> Children);
}
