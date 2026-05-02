using Catalog.Application.Common.Models;
using Catalog.Application.Storefront.DTOs;
using MediatR;

namespace Catalog.Application.Storefront.Queries.SearchStorefrontProducts
{
    public sealed record SearchStorefrontProductsQuery(
        Guid StoreId,
        string CurrencyCode,
        string? SearchTerm,
        Guid? CategoryId,
        Guid? BrandId,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<PagedResult<StorefrontProductSummaryDto>>;
}
