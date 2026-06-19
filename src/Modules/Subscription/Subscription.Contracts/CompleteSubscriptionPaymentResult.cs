namespace Subscription.Contracts;

public sealed record CompleteSubscriptionPaymentResult(
    bool IsSuccess,
    Guid SubscriptionId,
    Guid TenantId,
    string PlanCode,
    string? ErrorMessage);
