using Catalog.Domain.Enums;

namespace Catalog.Application.Products.DTOs
{
    public sealed record ProductSummaryDto(
        Guid Id,
        Guid StoreId,
        string Name,
        string Slug,
        Guid? BrandId,
        ProductType ProductType,
        ProductStatus ProductStatus,
        bool IsPublished,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);
}
