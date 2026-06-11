using Subscription.Domain.Entities;

namespace Subscription.Domain.Repositories;

public interface IPlanRepository
{
    Task<Plan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Plan>> ListPublicAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Plan plan, CancellationToken cancellationToken = default);
}
