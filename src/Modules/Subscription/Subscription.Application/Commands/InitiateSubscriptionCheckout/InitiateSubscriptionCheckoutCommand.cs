using MediatR;

namespace Subscription.Application.Commands.InitiateSubscriptionCheckout;

public sealed record InitiateSubscriptionCheckoutCommand(
    Guid TenantId,
    string PlanCode,
    string StoreName,
    string StoreSlug,
    string BuyerEmail,
    string BuyerName,
    string BuyerPhone,
    string BuyerIdentityNumber,
    string BuyerIpAddress) : IRequest<InitiateSubscriptionCheckoutResult>;

public sealed record InitiateSubscriptionCheckoutResult(
    Guid SubscriptionId,
    string PaymentPageUrl,
    string Token);
