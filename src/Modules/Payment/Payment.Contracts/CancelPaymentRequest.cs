namespace Payment.Contracts;

public sealed record CancelPaymentRequest(
    Guid StoreId,
    Guid PaymentId,
    string IdempotencyKey);
