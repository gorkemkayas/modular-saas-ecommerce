namespace Pricing.Application.Integrations;

public interface ICatalogSellableItemValidator
{
    Task<SellableItemValidationResult> ValidateAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        CancellationToken cancellationToken = default);
}
