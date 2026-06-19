using Microsoft.EntityFrameworkCore;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Infrastructure.Persistance.Repositories
{
    public sealed class StoreRepository : IStoreRepository
    {
        private readonly StoreDbContext _context;

        public StoreRepository(StoreDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Store.Domain.Stores.Store store, CancellationToken cancellationToken = default)
        {
            await _context.Stores.AddAsync(store, cancellationToken);
        }

        public Task<Store.Domain.Stores.Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.Stores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<Store.Domain.Stores.Store?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return _context.Stores.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        }

        public Task<Store.Domain.Stores.Store?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        {
            return _context.Stores.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
        }

        public async Task<IReadOnlyCollection<Store.Domain.Stores.Store>> ListPublishedAsync(
            int limit,
            CancellationToken cancellationToken = default)
        {
            return await _context.Stores
                .Where(x => x.IsPublished && x.Status == StoreStatus.Active)
                .OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
                .ThenBy(x => x.Name)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public Task<bool> ExistsByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return _context.Stores.AnyAsync(x => x.TenantId == tenantId, cancellationToken);
        }

        public Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        {
            return _context.Stores.AnyAsync(x => x.Slug == slug, cancellationToken);
        }

        public void Remove(Store.Domain.Stores.Store store)
        {
            _context.Stores.Remove(store);
        }
    }
}
