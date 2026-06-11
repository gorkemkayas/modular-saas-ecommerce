namespace Subscription.Contracts;

public sealed record PlanFeatureResult(
    string Key,
    bool IsEnabled,
    string? Description);
