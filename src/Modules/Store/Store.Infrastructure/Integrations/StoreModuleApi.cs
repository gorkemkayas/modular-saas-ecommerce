using Microsoft.EntityFrameworkCore;
using Store.Contracts;
using Store.Infrastructure.Persistance;

namespace Store.Infrastructure.Integrations
{
    public sealed class StoreModuleApi : IStoreModuleApi
    {
        private readonly StoreDbContext _context;

        public StoreModuleApi(StoreDbContext context)
        {
            _context = context;
        }

        public Task<StoreBranding?> GetBrandingAsync(
            Guid storeId,
            CancellationToken cancellationToken = default)
        {
            // Across modules the "store id" is the tenant id (TenantContext.TenantIdAsGuid),
            // while the Store aggregate has its own random Id. Match on TenantId, and fall
            // back to the aggregate Id in case a caller already holds the real store id.
            return _context.Stores
                .AsNoTracking()
                .Where(x => x.TenantId == storeId || x.Id == storeId)
                .OrderByDescending(x => x.TenantId == storeId)
                .Select(x => new StoreBranding(x.Id, x.Name, x.LogoUrl))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
