using Catalog.Contracts;
using Order.Application.Integrations;

namespace Order.Infrastructure.Integrations.Catalog;

public sealed class OrderCatalogProductService : IOrderCatalogProductService
{
    private readonly ICatalogModuleApi _catalogModuleApi;

    public OrderCatalogProductService(ICatalogModuleApi catalogModuleApi)
    {
        _catalogModuleApi = catalogModuleApi;
    }

    public async Task<OrderSellableItem?> GetSellableItemAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        CancellationToken cancellationToken = default)
    {
        var product = await _catalogModuleApi.GetSellableItemAsync(
            new GetCatalogSellableItemRequest(storeId, productId, productVariantId),
            cancellationToken);

        return product is null
            ? null
            : new OrderSellableItem(
                product.ProductId,
                product.ProductVariantId,
                product.ProductName,
                product.VariantName,
                product.Sku,
                product.ImageUrl);
    }
}
