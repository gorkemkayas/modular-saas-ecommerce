namespace ECommerce.API.Contracts.Catalog.Categories
{
    public sealed record CreateCategoryRequest(
        string Name,
        string Slug,
        string? Description,
        Guid? ParentCategoryId,
        int SortOrder);

    public sealed record UpdateCategoryRequest(
        string Name,
        string Slug,
        string? Description,
        int SortOrder);

    public sealed record ChangeCategoryParentRequest(Guid? ParentCategoryId);
}
