using Pricing.Domain.Enums;

namespace Pricing.Application.PriceLists.DTOs;

public sealed record PriceListSearchCriteria(
    Guid StoreId,
    string? CurrencyCode,
    PriceListStatus? Status,
    int PageNumber,
    int PageSize);
