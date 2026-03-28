namespace Catalog.Application.Categories.DTOs
{
    public sealed record CategoryDto(
        Guid Id,
        Guid StoreId,
        string Name,
        string Slug,
        string? Description,
        Guid? ParentCategoryId,
        bool IsActive,
        int SortOrder,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);
}
