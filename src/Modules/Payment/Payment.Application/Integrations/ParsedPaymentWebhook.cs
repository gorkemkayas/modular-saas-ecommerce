using Payment.Domain.Enums;

namespace Payment.Application.Integrations;

public sealed record ParsedPaymentWebhook(
    PaymentProvider Provider,
    PaymentGatewayOutcome Outcome,
    string IdempotencyKey,
    string? ExternalPaymentReference,
    string? ExternalConversationId,
    string? FailureCode,
    string? FailureMessage,
    decimal? RefundAmount);
