using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Common.Models;
using Catalog.Application.Storefront.DTOs;
using MediatR;

namespace Catalog.Application.Storefront.Queries.SearchStorefrontProducts
{
    public sealed class SearchStorefrontProductsQueryHandler
        : IRequestHandler<SearchStorefrontProductsQuery, PagedResult<StorefrontProductSummaryDto>>
    {
        private readonly IStorefrontCatalogReadService _storefrontCatalogReadService;

        public SearchStorefrontProductsQueryHandler(IStorefrontCatalogReadService storefrontCatalogReadService)
        {
            _storefrontCatalogReadService = storefrontCatalogReadService;
        }

        public Task<PagedResult<StorefrontProductSummaryDto>> Handle(
            SearchStorefrontProductsQuery query,
            CancellationToken cancellationToken)
        {
            var criteria = new StorefrontProductSearchCriteria(
                query.StoreId,
                query.CurrencyCode,
                query.SearchTerm,
                query.CategoryId,
                query.BrandId,
                query.PageNumber <= 0 ? 1 : query.PageNumber,
                query.PageSize <= 0 ? 20 : query.PageSize);

            return _storefrontCatalogReadService.SearchProductsAsync(criteria, cancellationToken);
        }
    }
}
