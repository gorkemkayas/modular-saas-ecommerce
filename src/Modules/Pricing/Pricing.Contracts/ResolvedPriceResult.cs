namespace Pricing.Contracts;

public sealed record ResolvedPriceResult(
    Guid StoreId,
    Guid ProductId,
    Guid? ProductVariantId,
    decimal Amount,
    string CurrencyCode,
    decimal? CompareAtAmount,
    Guid PriceListId,
    Guid PriceEntryId);
