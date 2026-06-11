namespace Subscription.Contracts;

public sealed record QuotaResult(
    Guid TenantId,
    string QuotaKey,
    int? Limit);
