namespace Order.Application.Integrations;

public sealed record ResolvedOrderPrice(
    decimal Amount,
    string CurrencyCode,
    decimal? CompareAtAmount,
    Guid PriceListId,
    Guid PriceEntryId);
