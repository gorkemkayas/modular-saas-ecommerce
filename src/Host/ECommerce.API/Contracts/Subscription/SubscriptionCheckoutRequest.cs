namespace ECommerce.API.Contracts.Subscription;

public sealed record SubscriptionCheckoutRequest(
    int TenantId,
    string PlanCode,
    string BuyerEmail,
    string BuyerName,
    string BuyerPhone,
    string BuyerIdentityNumber);
