using Microsoft.EntityFrameworkCore;
using Subscription.Domain.Entities;
using Subscription.Domain.Repositories;

namespace Subscription.Infrastructure.Persistence.Repositories;

public sealed class PlanRepository : IPlanRepository
{
    private readonly SubscriptionDbContext _context;

    public PlanRepository(SubscriptionDbContext context)
    {
        _context = context;
    }

    public Task<Plan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = Plan.NormalizeCode(code);

        return BuildAggregateQuery()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Plan>> ListPublicAsync(CancellationToken cancellationToken = default)
    {
        return await BuildAggregateQuery()
            .Where(x => x.IsPublic && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Code)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        await _context.Plans.AddAsync(plan, cancellationToken);
    }

    private IQueryable<Plan> BuildAggregateQuery()
    {
        return _context.Plans
            .Include(x => x.Features)
            .Include(x => x.Quotas)
            .AsSplitQuery();
    }
}
