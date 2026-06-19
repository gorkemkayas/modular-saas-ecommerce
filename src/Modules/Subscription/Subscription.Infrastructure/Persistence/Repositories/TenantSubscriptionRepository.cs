using Microsoft.EntityFrameworkCore;
using Subscription.Domain.Entities;
using Subscription.Domain.Repositories;

namespace Subscription.Infrastructure.Persistence.Repositories;

public sealed class TenantSubscriptionRepository : ITenantSubscriptionRepository
{
    private readonly SubscriptionDbContext _context;

    public TenantSubscriptionRepository(SubscriptionDbContext context)
    {
        _context = context;
    }

    public Task<TenantSubscription?> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _context.TenantSubscriptions
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
    }

    public Task<TenantSubscription?> GetByExternalPaymentTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return _context.TenantSubscriptions
            .FirstOrDefaultAsync(x => x.ExternalPaymentToken == token, cancellationToken);
    }

    public async Task AddAsync(
        TenantSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        await _context.TenantSubscriptions.AddAsync(subscription, cancellationToken);
    }

    public void Remove(TenantSubscription subscription)
    {
        _context.TenantSubscriptions.Remove(subscription);
    }
}
