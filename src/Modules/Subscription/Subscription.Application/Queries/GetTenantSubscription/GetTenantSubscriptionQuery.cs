using MediatR;
using Subscription.Application.DTOs;

namespace Subscription.Application.Queries.GetTenantSubscription;

public sealed record GetTenantSubscriptionQuery(Guid TenantId) : IRequest<TenantSubscriptionDto?>;
