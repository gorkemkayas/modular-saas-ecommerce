namespace Catalog.Contracts;

public sealed record ValidateCatalogSellableItemRequest(
    Guid StoreId,
    Guid ProductId,
    Guid? ProductVariantId);
