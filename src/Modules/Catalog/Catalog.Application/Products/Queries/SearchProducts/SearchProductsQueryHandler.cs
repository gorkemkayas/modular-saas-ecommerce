using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Common.Models;
using Catalog.Application.Products.DTOs;
using MediatR;

namespace Catalog.Application.Products.Queries.SearchProducts
{
    public sealed class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, PagedResult<ProductSummaryDto>>
    {
        private readonly IProductReadService _productReadService;

        public SearchProductsQueryHandler(IProductReadService productReadService)
        {
            _productReadService = productReadService;
        }

        public Task<PagedResult<ProductSummaryDto>> Handle(SearchProductsQuery query, CancellationToken cancellationToken)
        {
            var criteria = new ProductSearchCriteria(
                query.StoreId,
                query.SearchTerm,
                query.Status,
                query.ProductType,
                query.IsPublished,
                query.CategoryId,
                query.BrandId,
                query.PageNumber <= 0 ? 1 : query.PageNumber,
                query.PageSize <= 0 ? 20 : query.PageSize);

            return _productReadService.SearchAsync(criteria, cancellationToken);
        }
    }
}
