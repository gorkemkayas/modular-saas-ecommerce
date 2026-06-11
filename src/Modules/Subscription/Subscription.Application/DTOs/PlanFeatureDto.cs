namespace Subscription.Application.DTOs;

public sealed record PlanFeatureDto(
    string Key,
    bool IsEnabled,
    string? Description);
