using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Common.Models;
using Catalog.Application.Products.DTOs;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.ReadServices
{
    public sealed class ProductReadService : IProductReadService
    {
        private readonly CatalogDbContext _context;

        public ProductReadService(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDto?> GetByIdAsync(Guid storeId, Guid productId, CancellationToken cancellationToken = default)
        {
            var product = await BuildDetailQuery()
                .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == productId, cancellationToken);

            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductDto?> GetBySlugAsync(Guid storeId, string slug, CancellationToken cancellationToken = default)
        {
            var normalizedSlug = slug.Trim().ToLowerInvariant();
            var slugValueObject = Domain.ValueObjects.Slug.Create(normalizedSlug);

            var product = await BuildDetailQuery()
                .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Slug == slugValueObject, cancellationToken);

            return product == null ? null : MapToDto(product);
        }

        public async Task<PagedResult<ProductSummaryDto>> SearchAsync(
            ProductSearchCriteria criteria,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(x => x.StoreId == criteria.StoreId);

            var normalizedSearchTerm = string.IsNullOrWhiteSpace(criteria.SearchTerm)
                ? null
                : criteria.SearchTerm.Trim().ToLowerInvariant();

            if (normalizedSearchTerm is not null)
            {
                query = query.Where(x =>
                    x.Name.ToLower().Contains(normalizedSearchTerm) ||
                    EF.Property<string>(x, nameof(Product.Slug)).ToLower().Contains(normalizedSearchTerm));
            }

            if (criteria.Status.HasValue)
                query = query.Where(x => x.ProductStatus == criteria.Status.Value);

            if (criteria.ProductType.HasValue)
                query = query.Where(x => x.ProductType == criteria.ProductType.Value);

            if (criteria.IsPublished.HasValue)
                query = query.Where(x => x.IsPublished == criteria.IsPublished.Value);

            if (criteria.BrandId.HasValue)
                query = query.Where(x => x.BrandId == criteria.BrandId.Value);

            if (criteria.CategoryId.HasValue)
                query = query.Where(x => x.Categories.Any(category => category.CategoryId == criteria.CategoryId.Value));

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenBy(x => x.Name)
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .Select(x => new ProductSummaryDto(
                    x.Id,
                    x.StoreId,
                    x.Name,
                    EF.Property<string>(x, nameof(Product.Slug)),
                    x.BrandId,
                    x.ProductType,
                    x.ProductStatus,
                    x.IsPublished,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc))
                .ToArrayAsync(cancellationToken);

            return new PagedResult<ProductSummaryDto>(
                items,
                criteria.PageNumber,
                criteria.PageSize,
                totalCount);
        }

        private IQueryable<Product> BuildDetailQuery()
        {
            return _context.Products
                .AsNoTracking()
                .Include(x => x.Variants)
                    .ThenInclude(x => x.AttributeValues)
                .Include(x => x.Categories)
                .Include(x => x.AttributeValues)
                .Include(x => x.MediaItems)
                .AsSplitQuery();
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto(
                product.Id,
                product.StoreId,
                product.Name,
                product.ShortDescription,
                product.Description,
                product.Slug.Value,
                product.BrandId,
                product.Sku?.Value,
                product.ProductType,
                product.ProductStatus,
                product.IsPublished,
                product.PublishedAtUtc,
                product.CreatedAtUtc,
                product.UpdatedAtUtc,
                product.Categories
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new ProductCategoryAssignmentDto(
                        x.CategoryId,
                        x.IsPrimary,
                        x.SortOrder))
                    .ToArray(),
                product.AttributeValues
                    .OrderBy(x => x.AttributeDefinitionId)
                    .Select(x => new ProductAttributeValueDto(
                        x.AttributeDefinitionId,
                        x.ProductId,
                        x.ProductVariantId,
                        x.Value))
                    .ToArray(),
                product.Variants
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new ProductVariantDto(
                        x.Id,
                        x.ProductId,
                        x.Sku.Value,
                        x.Name,
                        x.IsActive,
                        x.SortOrder,
                        x.AttributeValues
                            .OrderBy(value => value.AttributeDefinitionId)
                            .Select(value => new ProductAttributeValueDto(
                                value.AttributeDefinitionId,
                                value.ProductId,
                                value.ProductVariantId,
                                value.Value))
                            .ToArray()))
                    .ToArray(),
                product.MediaItems
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new ProductMediaDto(
                        x.Id,
                        x.ProductId,
                        x.ProductVariantId,
                        x.MediaType,
                        x.Url,
                        x.AltText,
                        x.IsMain,
                        x.SortOrder))
                    .ToArray());
        }
    }
}
