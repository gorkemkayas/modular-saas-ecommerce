using MediatR;

namespace Subscription.Application.Commands.ProvisionTenantSubscription;

public sealed record ProvisionTenantSubscriptionCommand(
    Guid TenantId,
    string? PlanCode) : IRequest<Guid>;
