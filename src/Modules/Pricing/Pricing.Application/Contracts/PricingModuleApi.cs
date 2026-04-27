using Pricing.Application.Abstractions.Queries;
using Pricing.Contracts;

namespace Pricing.Application.Contracts;

public sealed class PricingModuleApi : IPricingModuleApi
{
    private readonly IPriceCoverageReadService _priceCoverageReadService;
    private readonly IPriceResolutionReadService _priceResolutionReadService;

    public PricingModuleApi(
        IPriceCoverageReadService priceCoverageReadService,
        IPriceResolutionReadService priceResolutionReadService)
    {
        _priceCoverageReadService = priceCoverageReadService;
        _priceResolutionReadService = priceResolutionReadService;
    }

    public Task<PriceCoverageResult> CheckPriceCoverageAsync(
        CheckPriceCoverageRequest request,
        CancellationToken cancellationToken = default)
    {
        return _priceCoverageReadService.CheckCoverageAsync(request, cancellationToken);
    }

    public async Task<ResolvedPriceResult?> ResolvePriceAsync(
        ResolvePriceRequest request,
        CancellationToken cancellationToken = default)
    {
        var resolvedPrice = await _priceResolutionReadService.GetResolvedPriceAsync(
            request.StoreId,
            request.ProductId,
            request.ProductVariantId,
            request.CurrencyCode,
            cancellationToken);

        return resolvedPrice is null
            ? null
            : new ResolvedPriceResult(
                resolvedPrice.StoreId,
                resolvedPrice.ProductId,
                resolvedPrice.ProductVariantId,
                resolvedPrice.Amount,
                resolvedPrice.CurrencyCode,
                resolvedPrice.CompareAtAmount,
                resolvedPrice.PriceListId,
                resolvedPrice.PriceEntryId);
    }
}
