namespace Inventory.Application.Integrations;

public interface IInventoryCatalogService
{
    Task<InventorySellableItem?> GetSellableItemAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        CancellationToken cancellationToken = default);
}
