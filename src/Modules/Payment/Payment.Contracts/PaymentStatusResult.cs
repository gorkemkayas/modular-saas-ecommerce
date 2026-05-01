using Payment.Domain.Enums;

namespace Payment.Contracts;

public sealed record PaymentStatusResult(
    Guid PaymentId,
    Guid OrderId,
    PaymentStatus Status,
    decimal Amount,
    string CurrencyCode,
    string? ExternalPaymentReference,
    string? ExternalConversationId);
