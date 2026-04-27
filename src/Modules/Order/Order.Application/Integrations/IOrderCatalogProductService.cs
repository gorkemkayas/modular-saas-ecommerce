namespace Order.Application.Integrations;

public interface IOrderCatalogProductService
{
    Task<OrderSellableItem?> GetSellableItemAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        CancellationToken cancellationToken = default);
}
