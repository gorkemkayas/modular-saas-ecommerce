using Catalog.Application.Categories.DTOs;
using Catalog.Domain.Entities;

namespace Catalog.Application.Categories
{
    internal static class CategoryMappings
    {
        public static CategoryDto ToDto(this Category category)
        {
            return new CategoryDto(
                category.Id,
                category.StoreId,
                category.Name,
                category.Slug.Value,
                category.Description,
                category.ParentCategoryId,
                category.IsActive,
                category.SortOrder,
                category.CreatedAtUtc,
                category.UpdatedAtUtc);
        }
    }
}
