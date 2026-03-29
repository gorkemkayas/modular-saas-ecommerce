using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Brands.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
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

                var rows = await query
                    .OrderBy(x => x.Name)
                    .Select(x => new BrandRow(
                        x.Id,
                        x.StoreId,
                        x.Name,
                        x.Slug,
                        x.Description,
                        x.IsActive,
                        x.CreatedAtUtc,
                        x.UpdatedAtUtc))
                    .ToArrayAsync(cancellationToken);

                return rows
                    .Where(x => MatchesSearch(x.Name, x.Slug.Value, normalizedSearch))
                    .Select(MapBrand)
                    .ToArray();
            }

            var unfilteredRows = await query
                .OrderBy(x => x.Name)
                .Select(x => new BrandRow(
                    x.Id,
                    x.StoreId,
                    x.Name,
                    x.Slug,
                    x.Description,
                    x.IsActive,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc))
                .ToArrayAsync(cancellationToken);

            return unfilteredRows.Select(MapBrand).ToArray();
        }

        private static bool MatchesSearch(string name, string slug, string normalizedSearch)
        {
            return name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)
                || slug.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase);
        }

        private static BrandDto MapBrand(BrandRow row)
        {
            return new BrandDto(
                row.Id,
                row.StoreId,
                row.Name,
                row.Slug.Value,
                row.Description,
                row.IsActive,
                row.CreatedAtUtc,
                row.UpdatedAtUtc);
        }

        private sealed record BrandRow(
            Guid Id,
            Guid StoreId,
            string Name,
            Slug Slug,
            string? Description,
            bool IsActive,
            DateTime CreatedAtUtc,
            DateTime UpdatedAtUtc);
    }
}
