using MediatR;
using Subscription.Application.Commands.CompleteSubscriptionPayment;
using Subscription.Application.Commands.InitiateSubscriptionCheckout;
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
                x.MonthlyPriceAmount,
                x.Currency,
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

    public async Task<Subscription.Contracts.InitiateSubscriptionCheckoutResult> InitiateCheckoutAsync(
        InitiateSubscriptionCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new InitiateSubscriptionCheckoutCommand(
                request.TenantId,
                request.PlanCode,
                request.StoreName,
                request.StoreSlug,
                request.BuyerEmail,
                request.BuyerName,
                request.BuyerPhone,
                request.BuyerIdentityNumber,
                request.BuyerIpAddress),
            cancellationToken);

        return new Subscription.Contracts.InitiateSubscriptionCheckoutResult(
            result.SubscriptionId,
            result.PaymentPageUrl,
            result.Token);
    }

    public async Task<Subscription.Contracts.CompleteSubscriptionPaymentResult> CompletePaymentAsync(
        CompleteSubscriptionPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new CompleteSubscriptionPaymentCommand(request.Token),
            cancellationToken);

        return new Subscription.Contracts.CompleteSubscriptionPaymentResult(
            result.IsSuccess,
            result.SubscriptionId,
            result.TenantId,
            result.PlanCode,
            result.ErrorMessage);
    }
}
