using Subscription.Domain.Exceptions;

namespace Subscription.Domain.Entities;

public sealed class PlanQuota
{
    private PlanQuota()
    {
    }

    private PlanQuota(Guid id, Guid planId, string key, int? limit)
    {
        Id = id;
        PlanId = planId;
        Key = NormalizeKey(key);
        Limit = NormalizeLimit(limit);
    }

    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public string Key { get; private set; } = default!;
    public int? Limit { get; private set; }

    public static PlanQuota Create(Guid planId, string key, int? limit)
    {
        if (planId == Guid.Empty)
            throw new SubscriptionDomainException("Plan id is required.");

        return new PlanQuota(Guid.NewGuid(), planId, key, limit);
    }

    public void ChangeLimit(int? limit)
    {
        Limit = NormalizeLimit(limit);
    }

    private static int? NormalizeLimit(int? limit)
    {
        if (limit.HasValue && limit.Value < 0)
            throw new SubscriptionDomainException("Quota limit cannot be negative.");

        return limit;
    }

    private static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new SubscriptionDomainException("Quota key is required.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 120)
            throw new SubscriptionDomainException("Quota key cannot exceed 120 characters.");

        return normalized;
    }
}
