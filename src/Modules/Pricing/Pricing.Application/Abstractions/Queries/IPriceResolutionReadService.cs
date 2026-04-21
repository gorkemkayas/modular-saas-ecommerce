using Pricing.Application.Prices.DTOs;

namespace Pricing.Application.Abstractions.Queries;

public interface IPriceResolutionReadService
{
    Task<ResolvedPriceDto?> GetResolvedPriceAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        string currencyCode,
        CancellationToken cancellationToken = default);
}
