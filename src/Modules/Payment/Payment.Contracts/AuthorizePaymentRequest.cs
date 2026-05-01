namespace Payment.Contracts;

public sealed record AuthorizePaymentRequest(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId,
    string IdempotencyKey,
    string? ClientIpAddress = null);
