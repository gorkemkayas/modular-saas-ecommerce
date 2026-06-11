namespace Subscription.Contracts;

public sealed record TenantSubscriptionResult(
    Guid SubscriptionId,
    Guid TenantId,
    string PlanCode,
    string PlanName,
    string Status,
    DateTime StartedAtUtc,
    IReadOnlyCollection<PlanFeatureResult> Features,
    IReadOnlyCollection<PlanQuotaResult> Quotas);
