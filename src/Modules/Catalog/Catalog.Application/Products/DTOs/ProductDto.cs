using Catalog.Domain.Enums;

namespace Catalog.Application.Products.DTOs
{
    public sealed record ProductDto(
        Guid Id,
        Guid StoreId,
        string Name,
        string? ShortDescription,
        string? Description,
        string Slug,
        Guid? BrandId,
        string? Sku,
        ProductType ProductType,
        ProductStatus ProductStatus,
        bool IsPublished,
        DateTime? PublishedAtUtc,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc,
        IReadOnlyCollection<ProductCategoryAssignmentDto> Categories,
        IReadOnlyCollection<ProductAttributeValueDto> AttributeValues,
        IReadOnlyCollection<ProductVariantDto> Variants,
        IReadOnlyCollection<ProductMediaDto> MediaItems);
}
