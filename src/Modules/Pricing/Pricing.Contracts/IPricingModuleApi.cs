namespace Pricing.Contracts;

public interface IPricingModuleApi
{
    Task<PriceCoverageResult> CheckPriceCoverageAsync(
        CheckPriceCoverageRequest request,
        CancellationToken cancellationToken = default);
}
