namespace Subscription.Contracts;

public sealed record InitiateSubscriptionCheckoutResult(
    Guid SubscriptionId,
    string PaymentPageUrl,
    string Token);
