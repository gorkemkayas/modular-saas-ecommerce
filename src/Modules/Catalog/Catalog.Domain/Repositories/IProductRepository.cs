using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Repositories
{
    public interface IProductRepository
    {
        Task AddAsync(Product product, CancellationToken cancellationToken = default);
        Task<Product?> GetByIdAsync(Guid storeId, Guid productId, CancellationToken cancellationToken = default);
        Task<Product?> GetBySlugAsync(Guid storeId, Slug slug, CancellationToken cancellationToken = default);
        Task<bool> ExistsBySlugAsync(Guid storeId, Slug slug, Guid? excludedProductId = null, CancellationToken cancellationToken = default);
        Task<bool> ExistsBySkuAsync(
            Guid storeId,
            Sku sku,
            Guid? excludedProductId = null,
            Guid? excludedVariantId = null,
            CancellationToken cancellationToken = default);
    }
}
