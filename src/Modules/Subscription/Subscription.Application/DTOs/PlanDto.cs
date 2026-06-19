namespace Subscription.Application.DTOs;

public sealed record PlanDto(
    string Code,
    string Name,
    string? Description,
    int DisplayOrder,
    decimal MonthlyPriceAmount,
    string Currency,
    IReadOnlyCollection<PlanFeatureDto> Features,
    IReadOnlyCollection<PlanQuotaDto> Quotas);
