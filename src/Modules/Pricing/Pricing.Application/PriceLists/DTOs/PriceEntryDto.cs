namespace Pricing.Application.PriceLists.DTOs;

public sealed record PriceEntryDto(
    Guid Id,
    Guid ProductId,
    Guid? ProductVariantId,
    decimal Amount,
    string CurrencyCode,
    decimal? CompareAtAmount,
    bool IsActive);
