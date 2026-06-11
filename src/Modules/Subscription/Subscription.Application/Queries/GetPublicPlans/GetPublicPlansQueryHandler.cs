using MediatR;
using Subscription.Application.DTOs;
using Subscription.Domain.Repositories;

namespace Subscription.Application.Queries.GetPublicPlans;

public sealed class GetPublicPlansQueryHandler : IRequestHandler<GetPublicPlansQuery, IReadOnlyCollection<PlanDto>>
{
    private readonly IPlanRepository _planRepository;

    public GetPublicPlansQueryHandler(IPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<IReadOnlyCollection<PlanDto>> Handle(
        GetPublicPlansQuery query,
        CancellationToken cancellationToken)
    {
        var plans = await _planRepository.ListPublicAsync(cancellationToken);

        return plans
            .Select(x => x.ToDto())
            .ToArray();
    }
}
