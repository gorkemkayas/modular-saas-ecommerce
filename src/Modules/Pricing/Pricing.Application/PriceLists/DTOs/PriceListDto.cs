using Pricing.Domain.Enums;

namespace Pricing.Application.PriceLists.DTOs;

public sealed record PriceListDto(
    Guid Id,
    Guid StoreId,
    string Name,
    string CurrencyCode,
    int Priority,
    bool IsDefault,
    PriceListStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<PriceEntryDto> Entries);
