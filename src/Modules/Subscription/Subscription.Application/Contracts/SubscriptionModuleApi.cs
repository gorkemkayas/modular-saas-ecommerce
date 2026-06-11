using MediatR;
using Subscription.Application.Commands.ProvisionTenantSubscription;
using Subscription.Application.Queries.GetPublicPlans;
using Subscription.Application.Queries.GetTenantSubscription;
using Subscription.Contracts;

namespace Subscription.Application.Contracts;

public sealed class SubscriptionModuleApi : ISubscriptionModuleApi
{
    private readonly ISender _sender;

    public SubscriptionModuleApi(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IReadOnlyCollection<PlanResult>> GetPublicPlansAsync(
        CancellationToken cancellationToken = default)
    {
        var plans = await _sender.Send(new GetPublicPlansQuery(), cancellationToken);

        return plans
            .Select(x => new PlanResult(
                x.Code,
                x.Name,
                x.Description,
                x.DisplayOrder,
                x.Features
                    .Select(feature => new PlanFeatureResult(feature.Key, feature.IsEnabled, feature.Description))
                    .ToArray(),
                x.Quotas
                    .Select(quota => new PlanQuotaResult(quota.Key, quota.Limit))
                    .ToArray()))
            .ToArray();
    }

    public Task<Guid> ProvisionTenantSubscriptionAsync(
        ProvisionTenantSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        return _sender.Send(
            new ProvisionTenantSubscriptionCommand(request.TenantId, request.PlanCode),
            cancellationToken);
    }

    public async Task<TenantSubscriptionResult?> GetTenantSubscriptionAsync(
        GetTenantSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _sender.Send(
            new GetTenantSubscriptionQuery(request.TenantId),
            cancellationToken);

        return subscription is null
            ? null
            : new TenantSubscriptionResult(
                subscription.SubscriptionId,
                subscription.TenantId,
                subscription.PlanCode,
                subscription.PlanName,
                subscription.Status,
                subscription.StartedAtUtc,
                subscription.Features
                    .Select(feature => new PlanFeatureResult(feature.Key, feature.IsEnabled, feature.Description))
                    .ToArray(),
                subscription.Quotas
                    .Select(quota => new PlanQuotaResult(quota.Key, quota.Limit))
                    .ToArray());
    }

    public async Task<bool> HasFeatureAsync(
        FeatureAccessRequest request,
        CancellationToken cancellationToken = default)
    {
        var subscription = await GetTenantSubscriptionAsync(
            new GetTenantSubscriptionRequest(request.TenantId),
            cancellationToken);

        return subscription?.Features.Any(
            feature => feature.IsEnabled &&
                string.Equals(feature.Key, request.FeatureKey, StringComparison.OrdinalIgnoreCase)) == true;
    }

    public async Task<QuotaResult?> GetQuotaAsync(
        QuotaRequest request,
        CancellationToken cancellationToken = default)
    {
        var subscription = await GetTenantSubscriptionAsync(
            new GetTenantSubscriptionRequest(request.TenantId),
            cancellationToken);

        var quota = subscription?.Quotas.FirstOrDefault(
            item => string.Equals(item.Key, request.QuotaKey, StringComparison.OrdinalIgnoreCase));

        return quota is null
            ? null
            : new QuotaResult(request.TenantId, quota.Key, quota.Limit);
    }
}
