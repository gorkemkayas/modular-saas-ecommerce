using Pricing.Contracts;

namespace Pricing.Application.Abstractions.Queries;

public interface IPriceCoverageReadService
{
    Task<PriceCoverageResult> CheckCoverageAsync(
        CheckPriceCoverageRequest request,
        CancellationToken cancellationToken = default);
}
