namespace Subscription.Contracts;

public sealed record FeatureAccessRequest(
    Guid TenantId,
    string FeatureKey);
