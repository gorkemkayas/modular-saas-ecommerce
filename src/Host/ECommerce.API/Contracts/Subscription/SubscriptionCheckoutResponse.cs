namespace ECommerce.API.Contracts.Subscription;

public sealed record SubscriptionCheckoutResponse(
    Guid SubscriptionId,
    string PaymentPageUrl,
    string Token);
