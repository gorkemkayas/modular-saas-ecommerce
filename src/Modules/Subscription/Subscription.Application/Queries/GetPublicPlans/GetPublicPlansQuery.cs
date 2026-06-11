using MediatR;
using Subscription.Application.DTOs;

namespace Subscription.Application.Queries.GetPublicPlans;

public sealed record GetPublicPlansQuery : IRequest<IReadOnlyCollection<PlanDto>>;
