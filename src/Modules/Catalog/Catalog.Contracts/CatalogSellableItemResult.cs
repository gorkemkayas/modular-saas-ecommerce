namespace Catalog.Contracts;

public sealed record CatalogSellableItemResult(
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string Sku);
