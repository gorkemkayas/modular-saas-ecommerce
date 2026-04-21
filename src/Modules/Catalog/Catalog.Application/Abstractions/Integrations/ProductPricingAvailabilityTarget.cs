namespace Catalog.Application.Abstractions.Integrations;

public sealed record ProductPricingAvailabilityTarget(
    Guid ProductId,
    Guid? ProductVariantId);
