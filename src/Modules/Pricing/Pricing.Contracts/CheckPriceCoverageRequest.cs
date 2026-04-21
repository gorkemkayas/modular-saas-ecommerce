namespace Pricing.Contracts;

public sealed record CheckPriceCoverageRequest(
    Guid StoreId,
    IReadOnlyCollection<PriceCoverageTarget> Targets,
    string? CurrencyCode = null);
