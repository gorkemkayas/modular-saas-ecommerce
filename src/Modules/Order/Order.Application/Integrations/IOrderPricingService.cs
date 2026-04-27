namespace Order.Application.Integrations;

public interface IOrderPricingService
{
    Task<ResolvedOrderPrice?> ResolvePriceAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        string currencyCode,
        CancellationToken cancellationToken = default);
}
