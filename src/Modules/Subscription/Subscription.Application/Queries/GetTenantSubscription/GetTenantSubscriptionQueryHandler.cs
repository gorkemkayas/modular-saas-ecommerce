using MediatR;
using Subscription.Application.DTOs;
using Subscription.Domain.Repositories;

namespace Subscription.Application.Queries.GetTenantSubscription;

public sealed class GetTenantSubscriptionQueryHandler : IRequestHandler<GetTenantSubscriptionQuery, TenantSubscriptionDto?>
{
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;

    public GetTenantSubscriptionQueryHandler(
        ITenantSubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
    }

    public async Task<TenantSubscriptionDto?> Handle(
        GetTenantSubscriptionQuery query,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByTenantIdAsync(query.TenantId, cancellationToken);
        if (subscription is null)
            return null;

        var plan = await _planRepository.GetByCodeAsync(subscription.PlanCode, cancellationToken);
        return plan is null ? null : subscription.ToDto(plan);
    }
}
