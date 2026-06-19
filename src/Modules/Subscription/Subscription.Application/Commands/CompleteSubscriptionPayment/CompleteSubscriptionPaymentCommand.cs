using MediatR;

namespace Subscription.Application.Commands.CompleteSubscriptionPayment;

public sealed record CompleteSubscriptionPaymentCommand(
    string Token) : IRequest<CompleteSubscriptionPaymentResult>;

public sealed record CompleteSubscriptionPaymentResult(
    bool IsSuccess,
    Guid SubscriptionId,
    Guid TenantId,
    string PlanCode,
    string? ErrorMessage);
