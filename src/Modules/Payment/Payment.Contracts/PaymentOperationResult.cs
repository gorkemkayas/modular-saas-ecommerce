using Payment.Domain.Enums;

namespace Payment.Contracts;

public sealed record PaymentOperationResult(
    Guid PaymentId,
    PaymentStatus Status,
    string? ExternalPaymentReference,
    string? ExternalConversationId,
    string? ActionUrl,
    string? FailureCode,
    string? FailureMessage);
