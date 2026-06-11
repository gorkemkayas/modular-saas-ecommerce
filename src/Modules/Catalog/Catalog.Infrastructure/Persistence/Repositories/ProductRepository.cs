using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Repositories
{
    public sealed class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _context;

        public ProductRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(product, cancellationToken);
        }

        public Task<Product?> GetByIdAsync(Guid storeId, Guid productId, CancellationToken cancellationToken = default)
        {
            return BuildAggregateQuery()
                .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == productId, cancellationToken);
        }

        public Task<Product?> GetBySlugAsync(Guid storeId, Slug slug, CancellationToken cancellationToken = default)
        {
            return BuildAggregateQuery()
                .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Slug == slug, cancellationToken);
        }

        public Task<int> CountNonArchivedByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
        {
            return _context.Products.CountAsync(
                x => x.StoreId == storeId && x.ProductStatus != ProductStatus.Archived,
                cancellationToken);
        }

        public Task<bool> ExistsBySlugAsync(
            Guid storeId,
            Slug slug,
            Guid? excludedProductId = null,
            CancellationToken cancellationToken = default)
        {
            return _context.Products.AnyAsync(
                x => x.StoreId == storeId
                    && x.Slug == slug
                    && (!excludedProductId.HasValue || x.Id != excludedProductId.Value),
                cancellationToken);
        }

        public async Task<bool> ExistsBySkuAsync(
            Guid storeId,
            Sku sku,
            Guid? excludedProductId = null,
            Guid? excludedVariantId = null,
            CancellationToken cancellationToken = default)
        {
            var productLevelExists = await _context.Products.AnyAsync(
                x => x.StoreId == storeId
                    && x.Sku == sku
                    && (!excludedProductId.HasValue || x.Id != excludedProductId.Value),
                cancellationToken);

            if (productLevelExists)
                return true;

            return await (
                from variant in _context.ProductVariants
                join product in _context.Products on variant.ProductId equals product.Id
                where product.StoreId == storeId
                    && variant.Sku == sku
                    && (!excludedProductId.HasValue || product.Id != excludedProductId.Value)
                    && (!excludedVariantId.HasValue || variant.Id != excludedVariantId.Value)
                select variant.Id)
                .AnyAsync(cancellationToken);
        }

        private IQueryable<Product> BuildAggregateQuery()
        {
            return _context.Products
                .Include(x => x.Variants)
                    .ThenInclude(x => x.AttributeValues)
                .Include(x => x.Categories)
                .Include(x => x.AttributeValues)
                .Include(x => x.MediaItems)
                .AsSplitQuery();
        }
    }
}
