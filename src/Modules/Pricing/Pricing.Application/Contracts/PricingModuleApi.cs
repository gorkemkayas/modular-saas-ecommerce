using Pricing.Application.Abstractions.Queries;
using Pricing.Contracts;

namespace Pricing.Application.Contracts;

public sealed class PricingModuleApi : IPricingModuleApi
{
    private readonly IPriceCoverageReadService _priceCoverageReadService;

    public PricingModuleApi(IPriceCoverageReadService priceCoverageReadService)
    {
        _priceCoverageReadService = priceCoverageReadService;
    }

    public Task<PriceCoverageResult> CheckPriceCoverageAsync(
        CheckPriceCoverageRequest request,
        CancellationToken cancellationToken = default)
    {
        return _priceCoverageReadService.CheckCoverageAsync(request, cancellationToken);
    }
}
