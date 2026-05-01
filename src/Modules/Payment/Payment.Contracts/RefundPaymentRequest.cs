namespace Payment.Contracts;

public sealed record RefundPaymentRequest(
    Guid StoreId,
    Guid PaymentId,
    decimal Amount,
    string Reason,
    string IdempotencyKey);
