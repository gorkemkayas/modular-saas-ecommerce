namespace Subscription.Contracts;

public sealed record QuotaRequest(
    Guid TenantId,
    string QuotaKey);
