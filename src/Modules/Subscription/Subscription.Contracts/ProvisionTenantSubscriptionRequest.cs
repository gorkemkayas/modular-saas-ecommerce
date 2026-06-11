namespace Subscription.Contracts;

public sealed record ProvisionTenantSubscriptionRequest(
    Guid TenantId,
    string? PlanCode);
