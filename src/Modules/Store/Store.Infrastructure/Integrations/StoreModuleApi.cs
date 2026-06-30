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
            return _context.Stores
                .AsNoTracking()
                .Where(x => x.Id == storeId)
                .Select(x => new StoreBranding(x.Id, x.Name, x.LogoUrl))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
