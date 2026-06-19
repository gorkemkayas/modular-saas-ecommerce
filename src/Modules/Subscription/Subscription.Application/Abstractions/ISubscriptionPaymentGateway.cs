namespace Subscription.Application.Abstractions;

public interface ISubscriptionPaymentGateway
{
    Task<SubscriptionCheckoutResult> InitializeCheckoutAsync(
        SubscriptionCheckoutRequest request,
        CancellationToken cancellationToken = default);

    Task<SubscriptionPaymentVerificationResult> VerifyPaymentAsync(
        string token,
        CancellationToken cancellationToken = default);
}

public sealed record SubscriptionCheckoutRequest(
    Guid SubscriptionId,
    string PlanName,
    decimal Amount,
    string Currency,
    string BuyerEmail,
    string BuyerName,
    string BuyerPhone,
    string BuyerIdentityNumber,
    string BuyerIpAddress);

public sealed record SubscriptionCheckoutResult(
    bool IsSuccess,
    string? PaymentPageUrl,
    string? Token,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record SubscriptionPaymentVerificationResult(
    bool IsSuccess,
    string? ExternalPaymentId,
    string? ErrorCode,
    string? ErrorMessage);
