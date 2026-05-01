using Payment.Domain.Enums;

namespace Payment.Contracts;

public sealed record RefundPaymentResult(
    Guid PaymentId,
    PaymentStatus Status,
    decimal RefundedAmount);
