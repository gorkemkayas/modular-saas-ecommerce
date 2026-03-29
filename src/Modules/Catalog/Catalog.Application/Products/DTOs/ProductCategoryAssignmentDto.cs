namespace Catalog.Application.Products.DTOs
{
    public sealed record ProductCategoryAssignmentDto(
        Guid CategoryId,
        bool IsPrimary,
        int SortOrder);
}
