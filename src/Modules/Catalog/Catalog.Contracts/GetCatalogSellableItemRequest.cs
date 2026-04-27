namespace Catalog.Contracts;

public sealed record GetCatalogSellableItemRequest(
    Guid StoreId,
    Guid ProductId,
    Guid? ProductVariantId);
