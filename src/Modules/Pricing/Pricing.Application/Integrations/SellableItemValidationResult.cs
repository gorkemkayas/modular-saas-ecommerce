namespace Pricing.Application.Integrations;

public sealed record SellableItemValidationResult(
    bool ProductExists,
    bool VariantExists,
    bool VariantBelongsToProduct,
    bool VariantIsActive,
    CatalogSellableItemType ProductType);
