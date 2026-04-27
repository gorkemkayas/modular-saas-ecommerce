namespace Pricing.Contracts;

public interface IPricingModuleApi
{
    Task<PriceCoverageResult> CheckPriceCoverageAsync(
        CheckPriceCoverageRequest request,
        CancellationToken cancellationToken = default);

    Task<ResolvedPriceResult?> ResolvePriceAsync(
        ResolvePriceRequest request,
        CancellationToken cancellationToken = default);
}
