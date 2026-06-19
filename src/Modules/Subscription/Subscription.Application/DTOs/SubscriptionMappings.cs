using Subscription.Domain.Entities;

namespace Subscription.Application.DTOs;

internal static class SubscriptionMappings
{
    public static PlanDto ToDto(this Plan plan)
    {
        return new PlanDto(
            plan.Code,
            plan.Name,
            plan.Description,
            plan.DisplayOrder,
            plan.MonthlyPriceAmount,
            plan.Currency,
            plan.Features
                .OrderBy(x => x.Key)
                .Select(x => new PlanFeatureDto(x.Key, x.IsEnabled, x.Description))
                .ToArray(),
            plan.Quotas
                .OrderBy(x => x.Key)
                .Select(x => new PlanQuotaDto(x.Key, x.Limit))
                .ToArray());
    }

    public static TenantSubscriptionDto ToDto(this TenantSubscription subscription, Plan plan)
    {
        return new TenantSubscriptionDto(
            subscription.Id,
            subscription.TenantId,
            subscription.PlanCode,
            plan.Name,
            subscription.Status.ToString(),
            subscription.StartedAtUtc,
            plan.Features
                .OrderBy(x => x.Key)
                .Select(x => new PlanFeatureDto(x.Key, x.IsEnabled, x.Description))
                .ToArray(),
            plan.Quotas
                .OrderBy(x => x.Key)
                .Select(x => new PlanQuotaDto(x.Key, x.Limit))
                .ToArray());
    }
}
