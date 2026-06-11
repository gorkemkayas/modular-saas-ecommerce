using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Repositories
{
    public interface ICategoryRepository
    {
        Task AddAsync(Category category, CancellationToken cancellationToken = default);
        Task<Category?> GetByIdAsync(Guid storeId, Guid categoryId, CancellationToken cancellationToken = default);
        Task<int> CountActiveByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(Guid storeId, Guid categoryId, CancellationToken cancellationToken = default);
        Task<bool> ExistsBySlugAsync(Guid storeId, Slug slug, Guid? excludedCategoryId = null, CancellationToken cancellationToken = default);
        Task<bool> IsDescendantOfAsync(
            Guid storeId,
            Guid categoryId,
            Guid potentialAncestorCategoryId,
            CancellationToken cancellationToken = default);
    }
}
