using MediatR;
using Pricing.Application.Abstractions.Queries;
using Pricing.Application.Common.Models;
using Pricing.Application.PriceLists.DTOs;

namespace Pricing.Application.PriceLists.Queries.SearchPriceLists;

public sealed class SearchPriceListsQueryHandler : IRequestHandler<SearchPriceListsQuery, PagedResult<PriceListSummaryDto>>
{
    private readonly IPriceListReadService _priceListReadService;

    public SearchPriceListsQueryHandler(IPriceListReadService priceListReadService)
    {
        _priceListReadService = priceListReadService;
    }

    public Task<PagedResult<PriceListSummaryDto>> Handle(SearchPriceListsQuery query, CancellationToken cancellationToken)
    {
        var criteria = new PriceListSearchCriteria(
            query.StoreId,
            query.CurrencyCode,
            query.Status,
            query.PageNumber <= 0 ? 1 : query.PageNumber,
            query.PageSize <= 0 ? 20 : query.PageSize);

        return _priceListReadService.SearchAsync(criteria, cancellationToken);
    }
}
