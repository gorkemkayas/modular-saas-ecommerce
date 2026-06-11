namespace Subscription.Contracts;

public sealed record PlanQuotaResult(
    string Key,
    int? Limit);
