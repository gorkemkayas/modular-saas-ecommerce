namespace Subscription.Contracts;

public sealed record PlanResult(
    string Code,
    string Name,
    string? Description,
    int DisplayOrder,
    IReadOnlyCollection<PlanFeatureResult> Features,
    IReadOnlyCollection<PlanQuotaResult> Quotas);
