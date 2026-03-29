using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Repositories
{
    public interface IBrandRepository
    {
        Task AddAsync(Brand brand, CancellationToken cancellationToken = default);
        Task<Brand?> GetByIdAsync(Guid storeId, Guid brandId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(Guid storeId, Guid brandId, CancellationToken cancellationToken = default);
        Task<bool> ExistsBySlugAsync(Guid storeId, Slug slug, Guid? excludedBrandId = null, CancellationToken cancellationToken = default);
    }
}
