using Catalog.Application.Common.Models;
using Catalog.Application.Products.DTOs;
using Catalog.Domain.Enums;
using MediatR;

namespace Catalog.Application.Products.Queries.SearchProducts
{
    public sealed record SearchProductsQuery(
        Guid StoreId,
        string? SearchTerm,
        ProductStatus? Status,
        ProductType? ProductType,
        bool? IsPublished,
        Guid? CategoryId,
        Guid? BrandId,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<PagedResult<ProductSummaryDto>>;
}
