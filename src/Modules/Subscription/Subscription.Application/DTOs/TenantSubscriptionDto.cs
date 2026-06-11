namespace Subscription.Application.DTOs;

public sealed record TenantSubscriptionDto(
    Guid SubscriptionId,
    Guid TenantId,
    string PlanCode,
    string PlanName,
    string Status,
    DateTime StartedAtUtc,
    IReadOnlyCollection<PlanFeatureDto> Features,
    IReadOnlyCollection<PlanQuotaDto> Quotas);
