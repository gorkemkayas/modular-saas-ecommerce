using Pricing.Domain.Enums;

namespace ECommerce.API.Contracts.Pricing.PriceLists;

public sealed record SearchPriceListsRequest(
    string? CurrencyCode,
    PriceListStatus? Status,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record CreatePriceListRequest(
    string Name,
    string CurrencyCode,
    int Priority = 0,
    bool IsDefault = false);

public sealed record RenamePriceListRequest(string Name);

public sealed record ChangePriceListPriorityRequest(int Priority);

public sealed record SetProductPriceRequest(
    decimal Amount,
    decimal? CompareAtAmount);

public sealed record SetVariantPriceRequest(
    decimal Amount,
    decimal? CompareAtAmount);
