namespace Subscription.Contracts;

public sealed record PlanResult(
    string Code,
    string Name,
    string? Description,
    int DisplayOrder,
    decimal MonthlyPriceAmount,
    string Currency,
    IReadOnlyCollection<PlanFeatureResult> Features,
    IReadOnlyCollection<PlanQuotaResult> Quotas);
