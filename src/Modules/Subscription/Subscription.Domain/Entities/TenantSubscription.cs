using Subscription.Domain.Common;
using Subscription.Domain.Enums;
using Subscription.Domain.Exceptions;

namespace Subscription.Domain.Entities;

public sealed class TenantSubscription : IAggregateRoot
{
    private TenantSubscription()
    {
    }

    private TenantSubscription(Guid id, Guid tenantId, string planCode)
    {
        if (tenantId == Guid.Empty)
            throw new SubscriptionDomainException("Tenant id is required.");

        Id = id;
        TenantId = tenantId;
        PlanCode = Plan.NormalizeCode(planCode);
        Status = SubscriptionStatus.PendingPayment;
        StartedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = StartedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string PlanCode { get; private set; } = default!;
    public SubscriptionStatus Status { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? CurrentPeriodStartUtc { get; private set; }
    public DateTime? CurrentPeriodEndUtc { get; private set; }
    public string? ExternalPaymentToken { get; private set; }
    public string? ExternalSubscriptionReferenceCode { get; private set; }

    public static TenantSubscription Create(Guid tenantId, string planCode)
    {
        return new TenantSubscription(Guid.NewGuid(), tenantId, planCode);
    }

    public void ChangePlan(string planCode)
    {
        PlanCode = Plan.NormalizeCode(planCode);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetExternalPaymentToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new SubscriptionDomainException("Payment token is required.");

        ExternalPaymentToken = token.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetExternalSubscriptionReferenceCode(string referenceCode)
    {
        if (string.IsNullOrWhiteSpace(referenceCode))
            throw new SubscriptionDomainException("Subscription reference code is required.");

        ExternalSubscriptionReferenceCode = referenceCode.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == SubscriptionStatus.Active)
            return;

        Status = SubscriptionStatus.Active;
        CurrentPeriodStartUtc = DateTime.UtcNow;
        CurrentPeriodEndUtc = CurrentPeriodStartUtc.Value.AddMonths(1);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Suspend()
    {
        if (Status == SubscriptionStatus.Suspended)
            return;

        Status = SubscriptionStatus.Suspended;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled)
            return;

        Status = SubscriptionStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CancelledAtUtc.Value;
    }
}
