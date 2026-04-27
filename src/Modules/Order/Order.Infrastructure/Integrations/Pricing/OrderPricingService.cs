using Order.Application.Integrations;
using Pricing.Contracts;

namespace Order.Infrastructure.Integrations.Pricing;

public sealed class OrderPricingService : IOrderPricingService
{
    private readonly IPricingModuleApi _pricingModuleApi;

    public OrderPricingService(IPricingModuleApi pricingModuleApi)
    {
        _pricingModuleApi = pricingModuleApi;
    }

    public async Task<ResolvedOrderPrice?> ResolvePriceAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        string currencyCode,
        CancellationToken cancellationToken = default)
    {
        var result = await _pricingModuleApi.ResolvePriceAsync(
            new ResolvePriceRequest(storeId, productId, productVariantId, currencyCode),
            cancellationToken);

        return result is null
            ? null
            : new ResolvedOrderPrice(
                result.Amount,
                result.CurrencyCode,
                result.CompareAtAmount,
                result.PriceListId,
                result.PriceEntryId);
    }
}
