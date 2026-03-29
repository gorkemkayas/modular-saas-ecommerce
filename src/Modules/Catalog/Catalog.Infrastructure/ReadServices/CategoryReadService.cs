using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Categories.DTOs;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.ReadServices
{
    public sealed class CategoryReadService : ICategoryReadService
    {
        private readonly CatalogDbContext _context;

        public CategoryReadService(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryDto?> GetByIdAsync(Guid storeId, Guid categoryId, CancellationToken cancellationToken = default)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == categoryId, cancellationToken);

            return category == null
                ? null
                : new CategoryDto(
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

        public async Task<IReadOnlyCollection<CategoryTreeNodeDto>> GetTreeAsync(Guid storeId, CancellationToken cancellationToken = default)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(x => x.StoreId == storeId)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new CategoryTreeNodeBuilder(
                    x.Id,
                    x.StoreId,
                    x.Name,
                    x.Slug.Value,
                    x.Description,
                    x.ParentCategoryId,
                    x.IsActive,
                    x.SortOrder))
                .ToListAsync(cancellationToken);

            var lookup = categories.ToDictionary(x => x.Id);

            foreach (var category in categories)
            {
                if (category.ParentCategoryId.HasValue && lookup.TryGetValue(category.ParentCategoryId.Value, out var parent))
                    parent.Children.Add(category);
            }

            return categories
                .Where(x => !x.ParentCategoryId.HasValue)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => x.ToDto())
                .ToArray();
        }

        private sealed class CategoryTreeNodeBuilder
        {
            public CategoryTreeNodeBuilder(
                Guid id,
                Guid storeId,
                string name,
                string slug,
                string? description,
                Guid? parentCategoryId,
                bool isActive,
                int sortOrder)
            {
                Id = id;
                StoreId = storeId;
                Name = name;
                Slug = slug;
                Description = description;
                ParentCategoryId = parentCategoryId;
                IsActive = isActive;
                SortOrder = sortOrder;
            }

            public Guid Id { get; }
            public Guid StoreId { get; }
            public string Name { get; }
            public string Slug { get; }
            public string? Description { get; }
            public Guid? ParentCategoryId { get; }
            public bool IsActive { get; }
            public int SortOrder { get; }
            public List<CategoryTreeNodeBuilder> Children { get; } = new();

            public CategoryTreeNodeDto ToDto()
            {
                return new CategoryTreeNodeDto(
                    Id,
                    StoreId,
                    Name,
                    Slug,
                    Description,
                    ParentCategoryId,
                    IsActive,
                    SortOrder,
                    Children
                        .OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.Name)
                        .Select(x => x.ToDto())
                        .ToArray());
            }
        }
    }
}
