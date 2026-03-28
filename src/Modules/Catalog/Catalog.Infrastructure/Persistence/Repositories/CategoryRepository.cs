using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public sealed class CategoryRepository : ICategoryRepository
    {
        private readonly CatalogDbContext _context;

        public CategoryRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _context.Categories.AddAsync(category, cancellationToken);
        }

        public Task<Category?> GetByIdAsync(Guid storeId, Guid categoryId, CancellationToken cancellationToken = default)
        {
            return _context.Categories.FirstOrDefaultAsync(
                x => x.StoreId == storeId && x.Id == categoryId,
                cancellationToken);
        }

        public Task<bool> ExistsByIdAsync(Guid storeId, Guid categoryId, CancellationToken cancellationToken = default)
        {
            return _context.Categories.AnyAsync(
                x => x.StoreId == storeId && x.Id == categoryId,
                cancellationToken);
        }

        public Task<bool> ExistsBySlugAsync(
            Guid storeId,
            Slug slug,
            Guid? excludedCategoryId = null,
            CancellationToken cancellationToken = default)
        {
            return _context.Categories.AnyAsync(
                x => x.StoreId == storeId
                    && x.Slug == slug
                    && (!excludedCategoryId.HasValue || x.Id != excludedCategoryId.Value),
                cancellationToken);
        }

        public async Task<bool> IsDescendantOfAsync(
            Guid storeId,
            Guid categoryId,
            Guid potentialAncestorCategoryId,
            CancellationToken cancellationToken = default)
        {
            if (categoryId == Guid.Empty || potentialAncestorCategoryId == Guid.Empty || categoryId == potentialAncestorCategoryId)
                return false;

            Guid? currentCategoryId = categoryId;

            while (currentCategoryId.HasValue)
            {
                var parentId = await _context.Categories
                    .Where(x => x.StoreId == storeId && x.Id == currentCategoryId.Value)
                    .Select(x => x.ParentCategoryId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!parentId.HasValue)
                    return false;

                if (parentId.Value == potentialAncestorCategoryId)
                    return true;

                currentCategoryId = parentId;
            }

            return false;
        }
    }
}
