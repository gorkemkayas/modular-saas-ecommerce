using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public sealed class BrandRepository : IBrandRepository
    {
        private readonly CatalogDbContext _context;

        public BrandRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            await _context.Brands.AddAsync(brand, cancellationToken);
        }

        public Task<Brand?> GetByIdAsync(Guid storeId, Guid brandId, CancellationToken cancellationToken = default)
        {
            return _context.Brands.FirstOrDefaultAsync(
                x => x.StoreId == storeId && x.Id == brandId,
                cancellationToken);
        }

        public Task<bool> ExistsByIdAsync(Guid storeId, Guid brandId, CancellationToken cancellationToken = default)
        {
            return _context.Brands.AnyAsync(
                x => x.StoreId == storeId && x.Id == brandId,
                cancellationToken);
        }

        public Task<bool> ExistsBySlugAsync(
            Guid storeId,
            Slug slug,
            Guid? excludedBrandId = null,
            CancellationToken cancellationToken = default)
        {
            return _context.Brands.AnyAsync(
                x => x.StoreId == storeId
                    && x.Slug == slug
                    && (!excludedBrandId.HasValue || x.Id != excludedBrandId.Value),
                cancellationToken);
        }
    }
}
