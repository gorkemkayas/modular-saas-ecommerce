using Notification.Application.Notifications.Services;
using Notification.Contracts;
using Notification.Domain.Enums;

namespace Notification.Application.Contracts;

public sealed class NotificationModuleApi : INotificationModuleApi
{
    private readonly INotificationSender _notificationSender;

    public NotificationModuleApi(INotificationSender notificationSender)
    {
        _notificationSender = notificationSender;
    }

    public Task<Guid> SendOrderPlacedAsync(
        SendOrderPlacedNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _notificationSender.SendAsync(
            new TransactionalNotificationRequest(
                request.StoreId,
                NotificationChannel.Email,
                NotificationTrigger.OrderPlaced,
                request.Locale,
                "Order",
                request.OrderId,
                request.CustomerId,
                request.RecipientEmail,
                request.RecipientName,
                new Dictionary<string, string?>
                {
                    ["RecipientName"] = request.RecipientName,
                    ["OrderNumber"] = request.OrderNumber,
                    ["GrandTotalAmount"] = request.GrandTotalAmount.ToString("0.00"),
                    ["CurrencyCode"] = request.CurrencyCode
                }),
            cancellationToken);
    }

    public Task<Guid> SendOrderCancelledAsync(
        SendOrderCancelledNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _notificationSender.SendAsync(
            new TransactionalNotificationRequest(
                request.StoreId,
                NotificationChannel.Email,
                NotificationTrigger.OrderCancelled,
                request.Locale,
                "Order",
                request.OrderId,
                request.CustomerId,
                request.RecipientEmail,
                request.RecipientName,
                new Dictionary<string, string?>
                {
                    ["RecipientName"] = request.RecipientName,
                    ["OrderNumber"] = request.OrderNumber,
                    ["CancellationReason"] = request.CancellationReason
                }),
            cancellationToken);
    }

    public Task<Guid> SendPaymentAuthorizedAsync(
        SendPaymentAuthorizedNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendPaymentAsync(
            request.StoreId,
            request.PaymentId,
            request.CustomerId,
            request.RecipientEmail,
            request.RecipientName,
            request.OrderNumber,
            request.Amount,
            request.CurrencyCode,
            request.PaymentReference,
            request.Locale,
            NotificationTrigger.PaymentAuthorized,
            cancellationToken);
    }

    public Task<Guid> SendPaymentCapturedAsync(
        SendPaymentCapturedNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendPaymentAsync(
            request.StoreId,
            request.PaymentId,
            request.CustomerId,
            request.RecipientEmail,
            request.RecipientName,
            request.OrderNumber,
            request.Amount,
            request.CurrencyCode,
            request.PaymentReference,
            request.Locale,
            NotificationTrigger.PaymentCaptured,
            cancellationToken);
    }

    public Task<Guid> SendPaymentFailedAsync(
        SendPaymentFailedNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _notificationSender.SendAsync(
            new TransactionalNotificationRequest(
                request.StoreId,
                NotificationChannel.Email,
                NotificationTrigger.PaymentFailed,
                request.Locale,
                "Payment",
                request.PaymentId,
                request.CustomerId,
                request.RecipientEmail,
                request.RecipientName,
                new Dictionary<string, string?>
                {
                    ["RecipientName"] = request.RecipientName,
                    ["OrderNumber"] = request.OrderNumber,
                    ["PaymentAmount"] = request.Amount.ToString("0.00"),
                    ["CurrencyCode"] = request.CurrencyCode,
                    ["FailureMessage"] = request.FailureMessage
                }),
            cancellationToken);
    }

    public Task<Guid> SendPaymentRefundedAsync(
        SendPaymentRefundedNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _notificationSender.SendAsync(
            new TransactionalNotificationRequest(
                request.StoreId,
                NotificationChannel.Email,
                NotificationTrigger.PaymentRefunded,
                request.Locale,
                "Payment",
                request.PaymentId,
                request.CustomerId,
                request.RecipientEmail,
                request.RecipientName,
                new Dictionary<string, string?>
                {
                    ["RecipientName"] = request.RecipientName,
                    ["OrderNumber"] = request.OrderNumber,
                    ["RefundedAmount"] = request.RefundedAmount.ToString("0.00"),
                    ["CurrencyCode"] = request.CurrencyCode
                }),
            cancellationToken);
    }

    public Task<Guid> SendShipmentCreatedAsync(
        SendShipmentCreatedNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendShipmentAsync(
            request.StoreId,
            request.ShipmentId,
            request.CustomerId,
            request.RecipientEmail,
            request.RecipientName,
            request.OrderNumber,
            request.ShipmentNumber,
            request.CarrierName,
            request.TrackingNumber,
            request.TrackingUrl,
            request.Locale,
            NotificationTrigger.ShipmentCreated,
            null,
            cancellationToken);
    }

    public Task<Guid> SendShipmentShippedAsync(
        SendShipmentShippedNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendShipmentAsync(
            request.StoreId,
            request.ShipmentId,
            request.CustomerId,
            request.RecipientEmail,
            request.RecipientName,
            request.OrderNumber,
            request.ShipmentNumber,
            request.CarrierName,
            request.TrackingNumber,
            request.TrackingUrl,
            request.Locale,
            NotificationTrigger.ShipmentShipped,
            null,
            cancellationToken);
    }

    public Task<Guid> SendShipmentDeliveredAsync(
        SendShipmentDeliveredNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendShipmentAsync(
            request.StoreId,
            request.ShipmentId,
            request.CustomerId,
            request.RecipientEmail,
            request.RecipientName,
            request.OrderNumber,
            request.ShipmentNumber,
            request.CarrierName,
            request.TrackingNumber,
            request.TrackingUrl,
            request.Locale,
            NotificationTrigger.ShipmentDelivered,
            null,
            cancellationToken);
    }

    public Task<Guid> SendShipmentDeliveryExceptionAsync(
        SendShipmentDeliveryExceptionNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendShipmentAsync(
            request.StoreId,
            request.ShipmentId,
            request.CustomerId,
            request.RecipientEmail,
            request.RecipientName,
            request.OrderNumber,
            request.ShipmentNumber,
            request.CarrierName,
            request.TrackingNumber,
            request.TrackingUrl,
            request.Locale,
            NotificationTrigger.ShipmentDeliveryException,
            request.Description,
            cancellationToken);
    }

    private Task<Guid> SendPaymentAsync(
        Guid storeId,
        Guid paymentId,
        Guid customerId,
        string recipientEmail,
        string recipientName,
        string orderNumber,
        decimal amount,
        string currencyCode,
        string? paymentReference,
        string locale,
        NotificationTrigger trigger,
        CancellationToken cancellationToken)
    {
        return _notificationSender.SendAsync(
            new TransactionalNotificationRequest(
                storeId,
                NotificationChannel.Email,
                trigger,
                locale,
                "Payment",
                paymentId,
                customerId,
                recipientEmail,
                recipientName,
                new Dictionary<string, string?>
                {
                    ["RecipientName"] = recipientName,
                    ["OrderNumber"] = orderNumber,
                    ["PaymentAmount"] = amount.ToString("0.00"),
                    ["CurrencyCode"] = currencyCode,
                    ["PaymentReference"] = paymentReference
                }),
            cancellationToken);
    }

    private Task<Guid> SendShipmentAsync(
        Guid storeId,
        Guid shipmentId,
        Guid customerId,
        string recipientEmail,
        string recipientName,
        string orderNumber,
        string shipmentNumber,
        string? carrierName,
        string? trackingNumber,
        string? trackingUrl,
        string locale,
        NotificationTrigger trigger,
        string? description,
        CancellationToken cancellationToken)
    {
        return _notificationSender.SendAsync(
            new TransactionalNotificationRequest(
                storeId,
                NotificationChannel.Email,
                trigger,
                locale,
                "Shipment",
                shipmentId,
                customerId,
                recipientEmail,
                recipientName,
                new Dictionary<string, string?>
                {
                    ["RecipientName"] = recipientName,
                    ["OrderNumber"] = orderNumber,
                    ["ShipmentNumber"] = shipmentNumber,
                    ["CarrierName"] = carrierName,
                    ["TrackingNumber"] = trackingNumber,
                    ["TrackingUrl"] = trackingUrl,
                    ["Description"] = description
                }),
            cancellationToken);
    }
}
