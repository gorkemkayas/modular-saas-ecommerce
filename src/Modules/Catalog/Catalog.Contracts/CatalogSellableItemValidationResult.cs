namespace Catalog.Contracts;

public sealed record CatalogSellableItemValidationResult(
    bool ProductExists,
    bool VariantExists,
    bool VariantBelongsToProduct,
    bool VariantIsActive,
    CatalogSellableItemType ProductType);
