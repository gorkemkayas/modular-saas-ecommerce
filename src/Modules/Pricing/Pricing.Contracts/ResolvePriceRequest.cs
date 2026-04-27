namespace Pricing.Contracts;

public sealed record ResolvePriceRequest(
    Guid StoreId,
    Guid ProductId,
    Guid? ProductVariantId,
    string CurrencyCode);
