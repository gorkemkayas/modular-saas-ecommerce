using MediatR;
using Microsoft.Extensions.Logging;
using Subscription.Application.Abstractions;
using Subscription.Application.Exceptions;
using Subscription.Contracts;
using Subscription.Domain.Entities;
using Subscription.Domain.Repositories;

namespace Subscription.Application.Commands.ProvisionTenantSubscription;

public sealed class ProvisionTenantSubscriptionCommandHandler : IRequestHandler<ProvisionTenantSubscriptionCommand, Guid>
{
    private readonly IPlanRepository _planRepository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProvisionTenantSubscriptionCommandHandler> _logger;

    public ProvisionTenantSubscriptionCommandHandler(
        IPlanRepository planRepository,
        ITenantSubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProvisionTenantSubscriptionCommandHandler> logger)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        ProvisionTenantSubscriptionCommand command,
        CancellationToken cancellationToken)
    {
        if (command.TenantId == Guid.Empty)
            throw new SubscriptionValidationException("TenantId is required.");

        var planCode = string.IsNullOrWhiteSpace(command.PlanCode)
            ? SubscriptionPlanCodes.Starter
            : Plan.NormalizeCode(command.PlanCode);

        var plan = await _planRepository.GetByCodeAsync(planCode, cancellationToken);

        if (plan is null || !plan.IsActive)
            throw new PlanNotFoundException(planCode);

        var existing = await _subscriptionRepository.GetByTenantIdAsync(command.TenantId, cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation(
                "Tenant subscription already exists | SubscriptionId: {SubscriptionId} | TenantId: {TenantId} | PlanCode: {PlanCode}",
                existing.Id,
                existing.TenantId,
                existing.PlanCode);

            return existing.Id;
        }

        var subscription = TenantSubscription.Create(command.TenantId, plan.Code);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tenant subscription provisioned | SubscriptionId: {SubscriptionId} | TenantId: {TenantId} | PlanCode: {PlanCode}",
            subscription.Id,
            subscription.TenantId,
            subscription.PlanCode);

        return subscription.Id;
    }
}
