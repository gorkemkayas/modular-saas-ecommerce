using Subscription.Domain.Exceptions;

namespace Subscription.Domain.Entities;

public sealed class PlanFeature
{
    private PlanFeature()
    {
    }

    private PlanFeature(Guid id, Guid planId, string key, bool isEnabled, string? description)
    {
        Id = id;
        PlanId = planId;
        Key = NormalizeKey(key);
        IsEnabled = isEnabled;
        Description = NormalizeOptional(description, 300);
    }

    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public string Key { get; private set; } = default!;
    public bool IsEnabled { get; private set; }
    public string? Description { get; private set; }

    public static PlanFeature Create(Guid planId, string key, bool isEnabled, string? description = null)
    {
        if (planId == Guid.Empty)
            throw new SubscriptionDomainException("Plan id is required.");

        return new PlanFeature(Guid.NewGuid(), planId, key, isEnabled, description);
    }

    public void Update(bool isEnabled, string? description)
    {
        IsEnabled = isEnabled;
        Description = NormalizeOptional(description, 300);
    }

    private static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new SubscriptionDomainException("Feature key is required.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 120)
            throw new SubscriptionDomainException("Feature key cannot exceed 120 characters.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new SubscriptionDomainException($"Value cannot exceed {maxLength} characters.");

        return normalized;
    }
}
