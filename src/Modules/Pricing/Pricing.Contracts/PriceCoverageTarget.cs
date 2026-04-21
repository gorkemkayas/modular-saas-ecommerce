namespace Pricing.Contracts;

public sealed record PriceCoverageTarget(
    Guid ProductId,
    Guid? ProductVariantId);
