namespace Catalog.Application.Abstractions.Integrations;

public interface IProductPricingAvailabilityChecker
{
    Task<bool> HasRequiredPricesAsync(
        Guid storeId,
        IReadOnlyCollection<ProductPricingAvailabilityTarget> targets,
        CancellationToken cancellationToken = default);
}
