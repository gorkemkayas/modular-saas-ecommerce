namespace Payment.Contracts;

public sealed record CapturePaymentRequest(
    Guid StoreId,
    Guid PaymentId,
    string IdempotencyKey);
