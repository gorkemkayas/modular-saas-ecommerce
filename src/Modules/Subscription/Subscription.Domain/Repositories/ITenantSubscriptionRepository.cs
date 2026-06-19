using Subscription.Domain.Entities;

namespace Subscription.Domain.Repositories;

public interface ITenantSubscriptionRepository
{
    Task<TenantSubscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<TenantSubscription?> GetByExternalPaymentTokenAsync(string token, CancellationToken cancellationToken = default);

    Task AddAsync(TenantSubscription subscription, CancellationToken cancellationToken = default);

    void Remove(TenantSubscription subscription);
}
