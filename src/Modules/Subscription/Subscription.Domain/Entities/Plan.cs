using Subscription.Domain.Common;
using Subscription.Domain.Exceptions;

namespace Subscription.Domain.Entities;

public sealed class Plan : IAggregateRoot
{
    private readonly List<PlanFeature> _features = new();
    private readonly List<PlanQuota> _quotas = new();

    private Plan()
    {
    }

    private Plan(
        Guid id,
        string code,
        string name,
        string? description,
        int displayOrder,
        bool isPublic,
        decimal monthlyPriceAmount,
        string currency)
    {
        if (monthlyPriceAmount < 0)
            throw new SubscriptionDomainException("Monthly price cannot be negative.");

        Id = id;
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, "Plan name", 120);
        Description = NormalizeOptional(description, 500);
        DisplayOrder = displayOrder;
        IsPublic = isPublic;
        MonthlyPriceAmount = monthlyPriceAmount;
        Currency = NormalizeRequired(currency, "Currency", 3);
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPublic { get; private set; }
    public decimal MonthlyPriceAmount { get; private set; }
    public string Currency { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public string? ExternalPricingPlanReferenceCode { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<PlanFeature> Features => _features.AsReadOnly();
    public IReadOnlyCollection<PlanQuota> Quotas => _quotas.AsReadOnly();

    public static Plan Create(
        string code,
        string name,
        string? description,
        int displayOrder,
        decimal monthlyPriceAmount,
        string currency,
        bool isPublic = true)
    {
        return new Plan(Guid.NewGuid(), code, name, description, displayOrder, isPublic, monthlyPriceAmount, currency);
    }

    public void UpdateMetadata(string name, string? description, int displayOrder, bool isPublic,
        decimal monthlyPriceAmount, string currency)
    {
        if (monthlyPriceAmount < 0)
            throw new SubscriptionDomainException("Monthly price cannot be negative.");

        Name = NormalizeRequired(name, "Plan name", 120);
        Description = NormalizeOptional(description, 500);
        DisplayOrder = displayOrder;
        IsPublic = isPublic;
        MonthlyPriceAmount = monthlyPriceAmount;
        Currency = NormalizeRequired(currency, "Currency", 3);
        Touch();
    }

    public void SetExternalPricingPlanReferenceCode(string referenceCode)
    {
        ExternalPricingPlanReferenceCode = referenceCode?.Trim();
        Touch();
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        Touch();
    }

    public void UpsertFeature(string key, bool isEnabled, string? description = null)
    {
        var normalizedKey = NormalizeCodeLikeKey(key, "Feature key");
        var existing = _features.FirstOrDefault(x => x.Key == normalizedKey);

        if (existing is null)
        {
            _features.Add(PlanFeature.Create(Id, normalizedKey, isEnabled, description));
        }
        else
        {
            existing.Update(isEnabled, description);
        }

        Touch();
    }

    public void UpsertQuota(string key, int? limit)
    {
        var normalizedKey = NormalizeCodeLikeKey(key, "Quota key");
        var existing = _quotas.FirstOrDefault(x => x.Key == normalizedKey);

        if (existing is null)
        {
            _quotas.Add(PlanQuota.Create(Id, normalizedKey, limit));
        }
        else
        {
            existing.ChangeLimit(limit);
        }

        Touch();
    }

    public static string NormalizeCode(string value)
    {
        return NormalizeCodeLikeKey(value, "Plan code");
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeCodeLikeKey(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new SubscriptionDomainException($"{fieldName} is required.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 120)
            throw new SubscriptionDomainException($"{fieldName} cannot exceed 120 characters.");

        return normalized;
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new SubscriptionDomainException($"{fieldName} is required.");

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new SubscriptionDomainException($"{fieldName} cannot exceed {maxLength} characters.");

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
