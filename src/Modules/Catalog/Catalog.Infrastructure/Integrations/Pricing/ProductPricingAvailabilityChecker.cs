using Catalog.Application.Abstractions.Integrations;
using Pricing.Contracts;

namespace Catalog.Infrastructure.Integrations.Pricing;

public sealed class ProductPricingAvailabilityChecker : IProductPricingAvailabilityChecker
{
    private readonly IPricingModuleApi _pricingModuleApi;

    public ProductPricingAvailabilityChecker(IPricingModuleApi pricingModuleApi)
    {
        _pricingModuleApi = pricingModuleApi;
    }

    public async Task<bool> HasRequiredPricesAsync(
        Guid storeId,
        IReadOnlyCollection<ProductPricingAvailabilityTarget> targets,
        CancellationToken cancellationToken = default)
    {
        if (storeId == Guid.Empty || targets.Count == 0)
            return false;

        var request = new CheckPriceCoverageRequest(
            storeId,
            targets
                .Select(x => new PriceCoverageTarget(x.ProductId, x.ProductVariantId))
                .ToArray());

        var result = await _pricingModuleApi.CheckPriceCoverageAsync(request, cancellationToken);
        return result.HasCoverage;
    }
}
