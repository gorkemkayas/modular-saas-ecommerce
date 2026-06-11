namespace ECommerce.API.Contracts.Catalog.Categories
{
    public sealed record CreateCategoryRequest(
        string Name,
        string Slug,
        string? Description,
        string? ImageUrl,
        Guid? ParentCategoryId,
        int SortOrder);

    public sealed record UpdateCategoryRequest(
        string Name,
        string Slug,
        string? Description,
        string? ImageUrl,
        int SortOrder);

    public sealed record ChangeCategoryParentRequest(Guid? ParentCategoryId);

    public sealed class UploadCategoryImageFileRequest
    {
        public IFormFile File { get; init; } = default!;
    }

    public sealed record UploadCategoryImageFileResponse(
        string Url,
        string OriginalFileName);
}
