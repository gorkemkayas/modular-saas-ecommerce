using MediatR;
using Microsoft.Extensions.Logging;
using Subscription.Application.Abstractions;
using Subscription.Application.Exceptions;
using Subscription.Domain.Entities;
using Subscription.Domain.Repositories;

namespace Subscription.Application.Commands.InitiateSubscriptionCheckout;

public sealed class InitiateSubscriptionCheckoutCommandHandler
    : IRequestHandler<InitiateSubscriptionCheckoutCommand, InitiateSubscriptionCheckoutResult>
{
    private readonly IPlanRepository _planRepository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InitiateSubscriptionCheckoutCommandHandler> _logger;

    public InitiateSubscriptionCheckoutCommandHandler(
        IPlanRepository planRepository,
        ITenantSubscriptionRepository subscriptionRepository,
        ISubscriptionPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork,
        ILogger<InitiateSubscriptionCheckoutCommandHandler> logger)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<InitiateSubscriptionCheckoutResult> Handle(
        InitiateSubscriptionCheckoutCommand command,
        CancellationToken cancellationToken)
    {
        if (command.TenantId == Guid.Empty)
            throw new SubscriptionValidationException("TenantId is required.");

        var planCode = Plan.NormalizeCode(command.PlanCode);
        var plan = await _planRepository.GetByCodeAsync(planCode, cancellationToken);

        if (plan is null || !plan.IsActive)
            throw new PlanNotFoundException(planCode);

        var existing = await _subscriptionRepository.GetByTenantIdAsync(command.TenantId, cancellationToken);

        TenantSubscription subscription;

        if (existing is not null)
        {
            if (existing.Status == Domain.Enums.SubscriptionStatus.Active)
                throw new SubscriptionValidationException("Tenant already has an active subscription.");

            existing.ChangePlan(planCode);
            subscription = existing;
        }
        else
        {
            subscription = TenantSubscription.Create(command.TenantId, planCode);
            await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        }

        var checkoutResult = await _paymentGateway.InitializeCheckoutAsync(
            new SubscriptionCheckoutRequest(
                subscription.Id,
                plan.Name,
                plan.MonthlyPriceAmount,
                plan.Currency,
                command.BuyerEmail,
                command.BuyerName,
                command.BuyerPhone,
                command.BuyerIdentityNumber,
                command.BuyerIpAddress),
            cancellationToken);

        if (!checkoutResult.IsSuccess)
            throw new SubscriptionValidationException(
                $"Payment initialization failed: {checkoutResult.ErrorMessage ?? checkoutResult.ErrorCode ?? "Unknown error"}");

        subscription.SetExternalPaymentToken(checkoutResult.Token!);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Subscription checkout initiated | SubscriptionId: {SubscriptionId} | TenantId: {TenantId} | PlanCode: {PlanCode}",
            subscription.Id,
            command.TenantId,
            planCode);

        return new InitiateSubscriptionCheckoutResult(
            subscription.Id,
            checkoutResult.PaymentPageUrl!,
            checkoutResult.Token!);
    }
}
