using Catalog.Application.Products.DTOs;
using Catalog.Domain.Entities;

namespace Catalog.Application.Products
{
    internal static class ProductMappings
    {
        public static ProductDto ToDto(this Product product)
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
                    .Select(x => new ProductCategoryAssignmentDto(x.CategoryId, x.IsPrimary, x.SortOrder))
                    .ToArray(),
                product.AttributeValues
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

        public static ProductSummaryDto ToSummaryDto(this Product product)
        {
            return new ProductSummaryDto(
                product.Id,
                product.StoreId,
                product.Name,
                product.Slug.Value,
                product.BrandId,
                product.ProductType,
                product.ProductStatus,
                product.IsPublished,
                product.CreatedAtUtc,
                product.UpdatedAtUtc);
        }
    }
}
