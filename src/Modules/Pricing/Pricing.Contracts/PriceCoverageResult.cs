namespace Pricing.Contracts;

public sealed record PriceCoverageResult(
    bool HasCoverage,
    IReadOnlyCollection<PriceCoverageTarget> MissingTargets);
