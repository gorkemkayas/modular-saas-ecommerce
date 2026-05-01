namespace Payment.Application.Integrations;

public interface IPaymentNotificationService
{
    Task SendPaymentAuthorizedAsync(
        Guid storeId,
        Guid paymentId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string recipientEmail,
        string recipientName,
        decimal amount,
        string currencyCode,
        string? paymentReference,
        CancellationToken cancellationToken = default);

    Task SendPaymentCapturedAsync(
        Guid storeId,
        Guid paymentId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string recipientEmail,
        string recipientName,
        decimal amount,
        string currencyCode,
        string? paymentReference,
        CancellationToken cancellationToken = default);

    Task SendPaymentFailedAsync(
        Guid storeId,
        Guid paymentId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string recipientEmail,
        string recipientName,
        decimal amount,
        string currencyCode,
        string? failureMessage,
        CancellationToken cancellationToken = default);

    Task SendPaymentRefundedAsync(
        Guid storeId,
        Guid paymentId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string recipientEmail,
        string recipientName,
        decimal refundedAmount,
        string currencyCode,
        CancellationToken cancellationToken = default);
}
