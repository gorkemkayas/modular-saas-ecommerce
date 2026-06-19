namespace Subscription.Contracts;

public sealed record InitiateSubscriptionCheckoutRequest(
    Guid TenantId,
    string PlanCode,
    string StoreName,
    string StoreSlug,
    string BuyerEmail,
    string BuyerName,
    string BuyerPhone,
    string BuyerIdentityNumber,
    string BuyerIpAddress);
