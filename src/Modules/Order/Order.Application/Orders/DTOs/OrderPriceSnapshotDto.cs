namespace Order.Application.Orders.DTOs;

public sealed record OrderPriceSnapshotDto(
    decimal Amount,
    string CurrencyCode,
    decimal? CompareAtAmount,
    Guid PriceListId,
    Guid PriceEntryId);
