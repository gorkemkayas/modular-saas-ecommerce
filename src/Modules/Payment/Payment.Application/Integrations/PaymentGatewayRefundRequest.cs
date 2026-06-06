namespace Payment.Application.Integrations;

public sealed record PaymentGatewayRefundRequest(
    Guid PaymentId,
    Guid StoreId,
    Guid OrderId,
    decimal Amount,
    decimal RefundAmount,
    string CurrencyCode,
    string Reason,
    string? ExternalPaymentReference,
    string? ExternalConversationId,
    string IdempotencyKey,
    Guid? ProviderAccountId = null);
