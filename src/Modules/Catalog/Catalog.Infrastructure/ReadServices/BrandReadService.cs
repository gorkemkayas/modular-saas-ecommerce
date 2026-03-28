using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Brands.DTOs;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.ReadServices
{
    public sealed class BrandReadService : IBrandReadService
    {
        private readonly CatalogDbContext _context;

        public BrandReadService(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<BrandDto?> GetByIdAsync(Guid storeId, Guid brandId, CancellationToken cancellationToken = default)
        {
            var brand = await _context.Brands
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == brandId, cancellationToken);

            return brand == null
                ? null
                : new BrandDto(
                    brand.Id,
                    brand.StoreId,
                    brand.Name,
                    brand.Slug.Value,
                    brand.Description,
                    brand.IsActive,
                    brand.CreatedAtUtc,
                    brand.UpdatedAtUtc);
        }

        public async Task<IReadOnlyCollection<BrandDto>> SearchAsync(
            Guid storeId,
            string? searchTerm,
            bool activeOnly,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Brands
                .AsNoTracking()
                .Where(x => x.StoreId == storeId);

            if (activeOnly)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearch = searchTerm.Trim().ToLowerInvariant();

                query = query.Where(x =>
                    x.Name.ToLower().Contains(normalizedSearch) ||
                    EF.Property<string>(x, nameof(Brand.Slug)).ToLower().Contains(normalizedSearch));
            }

            return await query
                .OrderBy(x => x.Name)
                .Select(x => new BrandDto(
                    x.Id,
                    x.StoreId,
                    x.Name,
                    EF.Property<string>(x, nameof(Brand.Slug)),
                    x.Description,
                    x.IsActive,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc))
                .ToArrayAsync(cancellationToken);
        }
    }
}
