using Notification.Contracts;
using Payment.Application.Integrations;

namespace Payment.Infrastructure.Integrations.Notification;

public sealed class PaymentNotificationService : IPaymentNotificationService
{
    private readonly INotificationModuleApi _notificationModuleApi;

    public PaymentNotificationService(INotificationModuleApi notificationModuleApi)
    {
        _notificationModuleApi = notificationModuleApi;
    }

    public Task SendPaymentAuthorizedAsync(
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
        CancellationToken cancellationToken = default)
    {
        return _notificationModuleApi.SendPaymentAuthorizedAsync(
            new SendPaymentAuthorizedNotificationRequest(
                storeId,
                paymentId,
                orderId,
                customerId,
                orderNumber,
                recipientEmail,
                recipientName,
                amount,
                currencyCode,
                paymentReference),
            cancellationToken);
    }

    public Task SendPaymentCapturedAsync(
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
        CancellationToken cancellationToken = default)
    {
        return _notificationModuleApi.SendPaymentCapturedAsync(
            new SendPaymentCapturedNotificationRequest(
                storeId,
                paymentId,
                orderId,
                customerId,
                orderNumber,
                recipientEmail,
                recipientName,
                amount,
                currencyCode,
                paymentReference),
            cancellationToken);
    }

    public Task SendPaymentFailedAsync(
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
        CancellationToken cancellationToken = default)
    {
        return _notificationModuleApi.SendPaymentFailedAsync(
            new SendPaymentFailedNotificationRequest(
                storeId,
                paymentId,
                orderId,
                customerId,
                orderNumber,
                recipientEmail,
                recipientName,
                amount,
                currencyCode,
                failureMessage),
            cancellationToken);
    }

    public Task SendPaymentRefundedAsync(
        Guid storeId,
        Guid paymentId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string recipientEmail,
        string recipientName,
        decimal refundedAmount,
        string currencyCode,
        CancellationToken cancellationToken = default)
    {
        return _notificationModuleApi.SendPaymentRefundedAsync(
            new SendPaymentRefundedNotificationRequest(
                storeId,
                paymentId,
                orderId,
                customerId,
                orderNumber,
                recipientEmail,
                recipientName,
                refundedAmount,
                currencyCode),
            cancellationToken);
    }
}
