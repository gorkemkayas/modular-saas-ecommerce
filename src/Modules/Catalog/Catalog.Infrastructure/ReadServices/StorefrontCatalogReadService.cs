using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Common.Models;
using Catalog.Application.Storefront.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.ReadServices
{
    public sealed class StorefrontCatalogReadService : IStorefrontCatalogReadService
    {
        private readonly CatalogDbContext _context;

        public StorefrontCatalogReadService(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<StorefrontProductSummaryDto>> SearchProductsAsync(
            StorefrontProductSearchCriteria criteria,
            CancellationToken cancellationToken = default)
        {
            var query = BuildVisibleProductsQuery(criteria.StoreId);

            var normalizedSearchTerm = Normalize(criteria.SearchTerm);

            if (normalizedSearchTerm is not null)
            {
                query = query.Where(x =>
                    x.Name.ToLower().Contains(normalizedSearchTerm) ||
                    EF.Property<string>(x, nameof(Product.Slug)).ToLower().Contains(normalizedSearchTerm));
            }

            if (criteria.CategoryId.HasValue)
            {
                query = query.Where(x => x.Categories.Any(category => category.CategoryId == criteria.CategoryId.Value));
            }

            if (criteria.BrandId.HasValue)
            {
                query = query.Where(x => x.BrandId == criteria.BrandId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.PublishedAtUtc)
                .ThenBy(x => x.Name)
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .Select(x => new StorefrontProductSummaryDto(
                    x.Id,
                    x.Name,
                    EF.Property<string>(x, nameof(Product.Slug)),
                    x.ShortDescription,
                    x.BrandId,
                    x.BrandId.HasValue
                        ? _context.Brands
                            .Where(brand => brand.Id == x.BrandId.Value && brand.IsActive)
                            .Select(brand => brand.Name)
                            .FirstOrDefault()
                        : null,
                    x.ProductType,
                    x.PublishedAtUtc,
                    x.MediaItems
                        .Where(media => !media.ProductVariantId.HasValue)
                        .OrderByDescending(media => media.IsMain)
                        .ThenBy(media => media.SortOrder)
                        .Select(media => media.Url)
                        .FirstOrDefault()))
                .ToArrayAsync(cancellationToken);

            return new PagedResult<StorefrontProductSummaryDto>(
                items,
                criteria.PageNumber,
                criteria.PageSize,
                totalCount);
        }

        public async Task<StorefrontProductDto?> GetProductBySlugAsync(
            Guid storeId,
            string slug,
            CancellationToken cancellationToken = default)
        {
            var normalizedSlug = NormalizeRequired(slug);
            var slugValueObject = Domain.ValueObjects.Slug.Create(normalizedSlug);

            var product = await BuildVisibleProductDetailsQuery(storeId)
                .FirstOrDefaultAsync(x => x.Slug == slugValueObject, cancellationToken);

            if (product is null)
                return null;

            var activeVariantIds = product.Variants
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .ToHashSet();

            var categoryIds = product.Categories
                .Select(x => x.CategoryId)
                .Distinct()
                .ToArray();

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(x => x.StoreId == storeId && x.IsActive && categoryIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    Slug = x.Slug.Value
                })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var attributeDefinitionIds = product.AttributeValues
                .Select(x => x.AttributeDefinitionId)
                .Concat(product.Variants.SelectMany(x => x.AttributeValues.Select(value => value.AttributeDefinitionId)))
                .Distinct()
                .ToArray();

            var attributeDefinitions = await _context.AttributeDefinitions
                .AsNoTracking()
                .Where(x => x.StoreId == storeId && x.IsActive && attributeDefinitionIds.Contains(x.Id))
                .Select(x => new StorefrontAttributeDefinitionLookup(
                    x.Id,
                    x.Name,
                    x.Code.Value,
                    x.IsVariantDefining))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            string? brandName = null;

            if (product.BrandId.HasValue)
            {
                brandName = await _context.Brands
                    .AsNoTracking()
                    .Where(x => x.Id == product.BrandId.Value && x.StoreId == storeId && x.IsActive)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var storefrontCategories = product.Categories
                .Where(x => categories.ContainsKey(x.CategoryId))
                .OrderBy(x => x.SortOrder)
                .Select(x =>
                {
                    var category = categories[x.CategoryId];
                    return new StorefrontProductCategoryDto(
                        category.Id,
                        category.Name,
                        category.Slug,
                        x.IsPrimary,
                        x.SortOrder);
                })
                .ToArray();

            var productAttributes = product.AttributeValues
                .Select(x => MapAttribute(x.AttributeDefinitionId, x.Value, attributeDefinitions))
                .Where(x => x is not null)
                .Cast<StorefrontProductAttributeDto>()
                .OrderBy(x => x.Name)
                .ToArray();

            var mediaItems = product.MediaItems
                .Where(x => !x.ProductVariantId.HasValue || activeVariantIds.Contains(x.ProductVariantId.Value))
                .OrderBy(x => x.ProductVariantId.HasValue)
                .ThenByDescending(x => x.IsMain)
                .ThenBy(x => x.SortOrder)
                .Select(MapMedia)
                .ToArray();

            var variants = product.Variants
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(variant => new StorefrontProductVariantDto(
                    variant.Id,
                    variant.Name,
                    variant.AttributeValues
                        .Select(x => MapAttribute(x.AttributeDefinitionId, x.Value, attributeDefinitions))
                        .Where(x => x is not null)
                        .Cast<StorefrontProductAttributeDto>()
                        .OrderBy(x => x.Name)
                        .ToArray(),
                    product.MediaItems
                        .Where(media => media.ProductVariantId == variant.Id)
                        .OrderByDescending(media => media.IsMain)
                        .ThenBy(media => media.SortOrder)
                        .Select(MapMedia)
                        .ToArray()))
                .ToArray();

            return new StorefrontProductDto(
                product.Id,
                product.Name,
                product.ShortDescription,
                product.Description,
                product.Slug.Value,
                product.BrandId,
                brandName,
                product.ProductType,
                product.PublishedAtUtc,
                storefrontCategories,
                productAttributes,
                variants,
                mediaItems);
        }

        public async Task<IReadOnlyCollection<StorefrontCategoryTreeNodeDto>> GetCategoryTreeAsync(
            Guid storeId,
            CancellationToken cancellationToken = default)
        {
            var visibleProductIdsQuery = BuildVisibleProductsQuery(storeId).Select(x => x.Id);

            var visibleCategoryIds = await _context.ProductCategories
                .AsNoTracking()
                .Where(x => visibleProductIdsQuery.Contains(x.ProductId))
                .Select(x => x.CategoryId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (visibleCategoryIds.Count == 0)
                return Array.Empty<StorefrontCategoryTreeNodeDto>();

            var activeCategories = await _context.Categories
                .AsNoTracking()
                .Where(x => x.StoreId == storeId && x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new StorefrontCategoryNodeBuilder(
                    x.Id,
                    x.Name,
                    x.Slug.Value,
                    x.Description,
                    x.ParentCategoryId,
                    x.SortOrder))
                .ToListAsync(cancellationToken);

            var categoryLookup = activeCategories.ToDictionary(x => x.Id);
            var includedCategoryIds = new HashSet<Guid>(visibleCategoryIds);

            foreach (var categoryId in visibleCategoryIds)
            {
                var currentCategoryId = categoryId;

                while (categoryLookup.TryGetValue(currentCategoryId, out var current) && current.ParentCategoryId.HasValue)
                {
                    if (!includedCategoryIds.Add(current.ParentCategoryId.Value))
                        break;

                    currentCategoryId = current.ParentCategoryId.Value;
                }
            }

            foreach (var category in activeCategories.Where(x => includedCategoryIds.Contains(x.Id)))
            {
                if (category.ParentCategoryId.HasValue
                    && includedCategoryIds.Contains(category.ParentCategoryId.Value)
                    && categoryLookup.TryGetValue(category.ParentCategoryId.Value, out var parent))
                {
                    parent.Children.Add(category);
                }
            }

            return activeCategories
                .Where(x => includedCategoryIds.Contains(x.Id) && !x.ParentCategoryId.HasValue)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => x.ToDto())
                .ToArray();
        }

        public async Task<IReadOnlyCollection<StorefrontBrandDto>> SearchBrandsAsync(
            Guid storeId,
            string? searchTerm,
            CancellationToken cancellationToken = default)
        {
            var visibleProductsQuery = BuildVisibleProductsQuery(storeId);

            var brandCountsQuery =
                from product in visibleProductsQuery
                where product.BrandId.HasValue
                group product by product.BrandId!.Value into productGroup
                select new
                {
                    BrandId = productGroup.Key,
                    ProductCount = productGroup.Count()
                };

            var query =
                from brand in _context.Brands.AsNoTracking()
                join brandCount in brandCountsQuery on brand.Id equals brandCount.BrandId
                where brand.StoreId == storeId && brand.IsActive
                select new
                {
                    brand.Id,
                    brand.Name,
                    Slug = brand.Slug.Value,
                    brand.Description,
                    brandCount.ProductCount
                };

            var normalizedSearch = Normalize(searchTerm);

            if (normalizedSearch is not null)
            {
                query = query.Where(x =>
                    x.Name.ToLower().Contains(normalizedSearch) ||
                    x.Slug.ToLower().Contains(normalizedSearch));
            }

            return await query
                .OrderByDescending(x => x.ProductCount)
                .ThenBy(x => x.Name)
                .Select(x => new StorefrontBrandDto(
                    x.Id,
                    x.Name,
                    x.Slug,
                    x.Description,
                    x.ProductCount))
                .ToArrayAsync(cancellationToken);
        }

        public async Task<StorefrontCatalogFacetsDto> GetFacetsAsync(
            StorefrontCatalogFacetCriteria criteria,
            CancellationToken cancellationToken = default)
        {
            var filteredProductsQuery = ApplyStorefrontFilters(
                BuildVisibleProductsQuery(criteria.StoreId),
                criteria.SearchTerm,
                criteria.CategoryId,
                criteria.BrandId);

            var brandFacetQuery =
                from product in filteredProductsQuery
                where product.BrandId.HasValue
                group product by product.BrandId!.Value into productGroup
                join brand in _context.Brands.AsNoTracking().Where(x => x.StoreId == criteria.StoreId && x.IsActive)
                    on productGroup.Key equals brand.Id
                orderby productGroup.Count() descending, brand.Name
                select new StorefrontBrandFacetDto(
                    brand.Id,
                    brand.Name,
                    brand.Slug.Value,
                    productGroup.Count());

            var visibleProductIdsQuery = filteredProductsQuery.Select(x => x.Id);

            var productAttributeRows =
                from value in _context.ProductAttributeValues.AsNoTracking()
                where value.ProductId.HasValue && visibleProductIdsQuery.Contains(value.ProductId.Value)
                join definition in _context.AttributeDefinitions.AsNoTracking()
                    on value.AttributeDefinitionId equals definition.Id
                where definition.StoreId == criteria.StoreId && definition.IsActive && definition.IsFilterable
                select new StorefrontAttributeFacetRow(
                    value.ProductId!.Value,
                    definition.Id,
                    definition.Name,
                    definition.Code.Value,
                    value.Value);

            var variantAttributeRows =
                from variant in _context.ProductVariants.AsNoTracking()
                where variant.IsActive && visibleProductIdsQuery.Contains(variant.ProductId)
                join value in _context.ProductAttributeValues.AsNoTracking()
                    on variant.Id equals value.ProductVariantId
                join definition in _context.AttributeDefinitions.AsNoTracking()
                    on value.AttributeDefinitionId equals definition.Id
                where definition.StoreId == criteria.StoreId && definition.IsActive && definition.IsFilterable
                select new StorefrontAttributeFacetRow(
                    variant.ProductId,
                    definition.Id,
                    definition.Name,
                    definition.Code.Value,
                    value.Value);

            var attributeRows = await productAttributeRows
                .Concat(variantAttributeRows)
                .ToListAsync(cancellationToken);

            var brandFacets = await brandFacetQuery.ToArrayAsync(cancellationToken);

            var attributeFacets = attributeRows
                .GroupBy(x => new { x.AttributeDefinitionId, x.Name, x.Code })
                .OrderBy(x => x.Key.Name)
                .Select(group => new StorefrontAttributeFacetDto(
                    group.Key.AttributeDefinitionId,
                    group.Key.Name,
                    group.Key.Code,
                    group
                        .GroupBy(x => x.Value)
                        .OrderByDescending(x => x.Select(value => value.ProductId).Distinct().Count())
                        .ThenBy(x => x.Key)
                        .Select(x => new StorefrontFacetValueDto(
                            x.Key,
                            x.Select(value => value.ProductId).Distinct().Count()))
                        .ToArray()))
                .ToArray();

            return new StorefrontCatalogFacetsDto(brandFacets, attributeFacets);
        }

        private IQueryable<Product> BuildVisibleProductsQuery(Guid storeId)
        {
            return _context.Products
                .AsNoTracking()
                .Where(x =>
                    x.StoreId == storeId
                    && x.IsPublished
                    && x.ProductStatus == ProductStatus.Active
                    && (!x.BrandId.HasValue || _context.Brands.Any(brand =>
                        brand.StoreId == storeId
                        && brand.Id == x.BrandId.Value
                        && brand.IsActive))
                    && x.Categories.Any(category => _context.Categories.Any(storeCategory =>
                        storeCategory.StoreId == storeId
                        && storeCategory.Id == category.CategoryId
                        && storeCategory.IsActive)));
        }

        private IQueryable<Product> BuildVisibleProductDetailsQuery(Guid storeId)
        {
            return BuildVisibleProductsQuery(storeId)
                .Include(x => x.Variants)
                    .ThenInclude(x => x.AttributeValues)
                .Include(x => x.Categories)
                .Include(x => x.AttributeValues)
                .Include(x => x.MediaItems)
                .AsSplitQuery();
        }

        private static IQueryable<Product> ApplyStorefrontFilters(
            IQueryable<Product> query,
            string? searchTerm,
            Guid? categoryId,
            Guid? brandId)
        {
            var normalizedSearch = Normalize(searchTerm);

            if (normalizedSearch is not null)
            {
                query = query.Where(x =>
                    x.Name.ToLower().Contains(normalizedSearch) ||
                    EF.Property<string>(x, nameof(Product.Slug)).ToLower().Contains(normalizedSearch));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.Categories.Any(category => category.CategoryId == categoryId.Value));
            }

            if (brandId.HasValue)
            {
                query = query.Where(x => x.BrandId == brandId.Value);
            }

            return query;
        }

        private static StorefrontProductMediaDto MapMedia(ProductMedia media)
        {
            return new StorefrontProductMediaDto(
                media.Id,
                media.ProductVariantId,
                media.MediaType,
                media.Url,
                media.AltText,
                media.IsMain,
                media.SortOrder);
        }

        private static StorefrontProductAttributeDto? MapAttribute(
            Guid attributeDefinitionId,
            string value,
            IReadOnlyDictionary<Guid, StorefrontAttributeDefinitionLookup> attributeDefinitions)
        {
            if (!attributeDefinitions.TryGetValue(attributeDefinitionId, out var attributeDefinition))
                return null;

            return new StorefrontProductAttributeDto(
                attributeDefinition.Id,
                attributeDefinition.Name,
                attributeDefinition.Code,
                value,
                attributeDefinition.IsVariantDefining);
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToLowerInvariant();
        }

        private static string NormalizeRequired(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Slug cannot be empty.", nameof(value));

            return value.Trim().ToLowerInvariant();
        }

        private sealed record StorefrontAttributeDefinitionLookup(
            Guid Id,
            string Name,
            string Code,
            bool IsVariantDefining);

        private sealed record StorefrontAttributeFacetRow(
            Guid ProductId,
            Guid AttributeDefinitionId,
            string Name,
            string Code,
            string Value);

        private sealed class StorefrontCategoryNodeBuilder
        {
            public StorefrontCategoryNodeBuilder(
                Guid id,
                string name,
                string slug,
                string? description,
                Guid? parentCategoryId,
                int sortOrder)
            {
                Id = id;
                Name = name;
                Slug = slug;
                Description = description;
                ParentCategoryId = parentCategoryId;
                SortOrder = sortOrder;
            }

            public Guid Id { get; }
            public string Name { get; }
            public string Slug { get; }
            public string? Description { get; }
            public Guid? ParentCategoryId { get; }
            public int SortOrder { get; }
            public List<StorefrontCategoryNodeBuilder> Children { get; } = new();

            public StorefrontCategoryTreeNodeDto ToDto()
            {
                return new StorefrontCategoryTreeNodeDto(
                    Id,
                    Name,
                    Slug,
                    Description,
                    ParentCategoryId,
                    SortOrder,
                    Children
                        .OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.Name)
                        .Select(x => x.ToDto())
                        .ToArray());
            }
        }
    }
}
