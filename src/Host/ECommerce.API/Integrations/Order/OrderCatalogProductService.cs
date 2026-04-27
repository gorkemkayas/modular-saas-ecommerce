using Catalog.Application.Products.Queries.GetProductById;
using Catalog.Domain.Enums;
using MediatR;
using Order.Application.Integrations;

namespace ECommerce.API.Integrations.Order;

public sealed class OrderCatalogProductService : IOrderCatalogProductService
{
    private readonly ISender _sender;

    public OrderCatalogProductService(ISender sender)
    {
        _sender = sender;
    }

    public async Task<OrderSellableItem?> GetSellableItemAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        CancellationToken cancellationToken = default)
    {
        var product = await _sender.Send(new GetProductByIdQuery(storeId, productId), cancellationToken);
        if (product is null || !product.IsPublished || product.ProductStatus != ProductStatus.Active)
            return null;

        if (product.ProductType == ProductType.Variant)
        {
            if (!productVariantId.HasValue)
                return null;

            var variant = product.Variants.FirstOrDefault(x => x.Id == productVariantId.Value && x.IsActive);
            if (variant is null)
                return null;

            return new OrderSellableItem(
                product.Id,
                variant.Id,
                product.Name,
                variant.Name,
                variant.Sku);
        }

        if (productVariantId.HasValue)
            return null;

        return new OrderSellableItem(
            product.Id,
            null,
            product.Name,
            null,
            product.Sku);
    }
}
