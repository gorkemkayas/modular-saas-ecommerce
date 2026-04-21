using MediatR;
using Pricing.Application.Abstractions.Queries;
using Pricing.Application.PriceLists.DTOs;

namespace Pricing.Application.PriceLists.Queries.GetPriceListById;

public sealed class GetPriceListByIdQueryHandler : IRequestHandler<GetPriceListByIdQuery, PriceListDto?>
{
    private readonly IPriceListReadService _priceListReadService;

    public GetPriceListByIdQueryHandler(IPriceListReadService priceListReadService)
    {
        _priceListReadService = priceListReadService;
    }

    public Task<PriceListDto?> Handle(GetPriceListByIdQuery query, CancellationToken cancellationToken)
    {
        return _priceListReadService.GetByIdAsync(query.StoreId, query.PriceListId, cancellationToken);
    }
}
