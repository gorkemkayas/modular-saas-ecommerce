using MediatR;
using Pricing.Application.Abstractions.Queries;
using Pricing.Application.Prices.DTOs;

namespace Pricing.Application.Prices.Queries.GetResolvedPrice;

public sealed class GetResolvedPriceQueryHandler : IRequestHandler<GetResolvedPriceQuery, ResolvedPriceDto?>
{
    private readonly IPriceResolutionReadService _priceResolutionReadService;

    public GetResolvedPriceQueryHandler(IPriceResolutionReadService priceResolutionReadService)
    {
        _priceResolutionReadService = priceResolutionReadService;
    }

    public Task<ResolvedPriceDto?> Handle(GetResolvedPriceQuery query, CancellationToken cancellationToken)
    {
        return _priceResolutionReadService.GetResolvedPriceAsync(
            query.StoreId,
            query.ProductId,
            query.ProductVariantId,
            query.CurrencyCode,
            cancellationToken);
    }
}
