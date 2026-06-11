namespace Subscription.Application.DTOs;

public sealed record PlanQuotaDto(
    string Key,
    int? Limit);
