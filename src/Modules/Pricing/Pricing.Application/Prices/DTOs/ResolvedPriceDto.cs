namespace Pricing.Application.Prices.DTOs;

public sealed record ResolvedPriceDto(
    Guid StoreId,
    Guid ProductId,
    Guid? ProductVariantId,
    decimal Amount,
    string CurrencyCode,
    decimal? CompareAtAmount,
    Guid PriceListId,
    Guid PriceEntryId);
