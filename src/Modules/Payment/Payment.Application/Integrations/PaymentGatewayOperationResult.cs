namespace Payment.Application.Integrations;

public sealed record PaymentGatewayOperationResult(
    PaymentGatewayOutcome Outcome,
    string? ExternalPaymentReference,
    string? ExternalConversationId,
    string? ActionUrl,
    string? FailureCode,
    string? FailureMessage,
    string? ProviderRequestReference,
    decimal? RefundedAmount = null,
    Guid? ProviderAccountId = null);
