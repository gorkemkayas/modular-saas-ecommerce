using Store.Domain.ValueObjects;

namespace Store.Domain.Stores
{
    public interface IStoreRepository
    {
        Task AddAsync(Store store, CancellationToken cancellationToken = default);
        Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Store?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
        Task<Store?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<Store>> ListPublishedAsync(int limit, CancellationToken cancellationToken = default);
        Task<bool> ExistsByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
        Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default);
    }
}
