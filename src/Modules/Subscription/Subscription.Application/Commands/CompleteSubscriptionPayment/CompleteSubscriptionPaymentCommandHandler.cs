using MediatR;
using Microsoft.Extensions.Logging;
using Subscription.Application.Abstractions;
using Subscription.Application.Exceptions;
using Subscription.Domain.Enums;
using Subscription.Domain.Repositories;

namespace Subscription.Application.Commands.CompleteSubscriptionPayment;

public sealed class CompleteSubscriptionPaymentCommandHandler
    : IRequestHandler<CompleteSubscriptionPaymentCommand, CompleteSubscriptionPaymentResult>
{
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteSubscriptionPaymentCommandHandler> _logger;

    public CompleteSubscriptionPaymentCommandHandler(
        ITenantSubscriptionRepository subscriptionRepository,
        ISubscriptionPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork,
        ILogger<CompleteSubscriptionPaymentCommandHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CompleteSubscriptionPaymentResult> Handle(
        CompleteSubscriptionPaymentCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new SubscriptionValidationException("Payment token is required.");

        var subscription = await _subscriptionRepository.GetByExternalPaymentTokenAsync(
            command.Token, cancellationToken);

        if (subscription is null)
            throw new SubscriptionValidationException("No subscription found for the given payment token.");

        if (subscription.Status == SubscriptionStatus.Active)
        {
            return new CompleteSubscriptionPaymentResult(
                true, subscription.Id, subscription.TenantId, subscription.PlanCode, null);
        }

        var verification = await _paymentGateway.VerifyPaymentAsync(command.Token, cancellationToken);

        if (!verification.IsSuccess)
        {
            _logger.LogWarning(
                "Subscription payment verification failed | SubscriptionId: {SubscriptionId} | Error: {Error}",
                subscription.Id,
                verification.ErrorMessage ?? verification.ErrorCode);

            _subscriptionRepository.Remove(subscription);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CompleteSubscriptionPaymentResult(
                false, subscription.Id, subscription.TenantId, subscription.PlanCode,
                verification.ErrorMessage ?? "Payment verification failed.");
        }

        subscription.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Subscription payment completed | SubscriptionId: {SubscriptionId} | TenantId: {TenantId} | PlanCode: {PlanCode}",
            subscription.Id,
            subscription.TenantId,
            subscription.PlanCode);

        return new CompleteSubscriptionPaymentResult(
            true, subscription.Id, subscription.TenantId, subscription.PlanCode, null);
    }
}
