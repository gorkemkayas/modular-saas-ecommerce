using Catalog.Contracts;
using Inventory.Application.Integrations;

namespace Inventory.Infrastructure.Integrations.Catalog;

public sealed class InventoryCatalogService : IInventoryCatalogService
{
    private readonly ICatalogModuleApi _catalogModuleApi;

    public InventoryCatalogService(ICatalogModuleApi catalogModuleApi)
    {
        _catalogModuleApi = catalogModuleApi;
    }

    public async Task<InventorySellableItem?> GetSellableItemAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalogModuleApi.GetSellableItemAsync(
            new GetCatalogSellableItemRequest(storeId, productId, productVariantId),
            cancellationToken);

        return result is null
            ? null
            : new InventorySellableItem(
                result.ProductId,
                result.ProductVariantId,
                result.ProductName,
                result.VariantName,
                result.Sku);
    }
}
