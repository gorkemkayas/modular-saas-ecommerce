namespace Subscription.Contracts;

public interface ISubscriptionModuleApi
{
    Task<IReadOnlyCollection<PlanResult>> GetPublicPlansAsync(
        CancellationToken cancellationToken = default);

    Task<Guid> ProvisionTenantSubscriptionAsync(
        ProvisionTenantSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    Task<TenantSubscriptionResult?> GetTenantSubscriptionAsync(
        GetTenantSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> HasFeatureAsync(
        FeatureAccessRequest request,
        CancellationToken cancellationToken = default);

    Task<QuotaResult?> GetQuotaAsync(
        QuotaRequest request,
        CancellationToken cancellationToken = default);

    Task<InitiateSubscriptionCheckoutResult> InitiateCheckoutAsync(
        InitiateSubscriptionCheckoutRequest request,
        CancellationToken cancellationToken = default);

    Task<CompleteSubscriptionPaymentResult> CompletePaymentAsync(
        CompleteSubscriptionPaymentRequest request,
        CancellationToken cancellationToken = default);
}
