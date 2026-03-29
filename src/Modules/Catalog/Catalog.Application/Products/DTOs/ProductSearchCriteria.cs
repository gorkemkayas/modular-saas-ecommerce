using Catalog.Domain.Enums;

namespace Catalog.Application.Products.DTOs
{
    public sealed record ProductSearchCriteria(
        Guid StoreId,
        string? SearchTerm,
        ProductStatus? Status,
        ProductType? ProductType,
        bool? IsPublished,
        Guid? CategoryId,
        Guid? BrandId,
        int PageNumber,
        int PageSize);
}
