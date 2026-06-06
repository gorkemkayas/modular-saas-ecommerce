namespace Payment.Application.Integrations;

public sealed record PaymentGatewayCaptureRequest(
    Guid PaymentId,
    Guid StoreId,
    Guid OrderId,
    decimal Amount,
    string CurrencyCode,
    string? ExternalPaymentReference,
    string? ExternalConversationId,
    string IdempotencyKey,
    Guid? ProviderAccountId = null);
