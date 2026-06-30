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
        var lineItems = request.Items
            .Select(x => new EmailLineItem(
                x.Name,
                x.Variant,
                x.Quantity,
                FormatMoney(x.LineTotalAmount, request.CurrencyCode)))
            .ToArray();

        var totals = new List<EmailDetailRow>
        {
            new("Ara Toplam", FormatMoney(request.SubtotalAmount, request.CurrencyCode))
        };

        if (request.ShippingAmount > 0)
            totals.Add(new EmailDetailRow("Kargo", FormatMoney(request.ShippingAmount, request.CurrencyCode)));

        totals.Add(new EmailDetailRow("Toplam", FormatMoney(request.GrandTotalAmount, request.CurrencyCode)));

        var content = new EmailContent(
            CallToAction: new EmailCallToAction("Siparişi Görüntüle", $"/account/orders/{request.OrderId}"),
            LineItems: lineItems,
            Totals: totals);

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
                    ["GrandTotalAmount"] = FormatMoney(request.GrandTotalAmount, request.CurrencyCode),
                    ["CurrencyCode"] = request.CurrencyCode
                },
                content),
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
                },
                new EmailContent(
                    CallToAction: new EmailCallToAction("Siparişi Görüntüle", $"/account/orders/{request.OrderId}"),
                    Details: string.IsNullOrWhiteSpace(request.CancellationReason)
                        ? new[] { new EmailDetailRow("Sipariş No", request.OrderNumber) }
                        : new[]
                        {
                            new EmailDetailRow("Sipariş No", request.OrderNumber),
                            new EmailDetailRow("İptal Nedeni", request.CancellationReason!)
                        })),
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
                    ["PaymentAmount"] = FormatMoney(request.Amount, request.CurrencyCode),
                    ["CurrencyCode"] = request.CurrencyCode,
                    ["FailureMessage"] = request.FailureMessage
                },
                new EmailContent(
                    Details: string.IsNullOrWhiteSpace(request.FailureMessage)
                        ? new[]
                        {
                            new EmailDetailRow("Sipariş No", request.OrderNumber),
                            new EmailDetailRow("Tutar", FormatMoney(request.Amount, request.CurrencyCode))
                        }
                        : new[]
                        {
                            new EmailDetailRow("Sipariş No", request.OrderNumber),
                            new EmailDetailRow("Tutar", FormatMoney(request.Amount, request.CurrencyCode)),
                            new EmailDetailRow("Açıklama", request.FailureMessage!)
                        })),
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
                    ["RefundedAmount"] = FormatMoney(request.RefundedAmount, request.CurrencyCode),
                    ["CurrencyCode"] = request.CurrencyCode
                },
                new EmailContent(
                    Details: new[]
                    {
                        new EmailDetailRow("Sipariş No", request.OrderNumber),
                        new EmailDetailRow("İade Tutarı", FormatMoney(request.RefundedAmount, request.CurrencyCode))
                    })),
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
        var details = new List<EmailDetailRow>
        {
            new("Sipariş No", orderNumber),
            new("Tutar", FormatMoney(amount, currencyCode))
        };

        if (!string.IsNullOrWhiteSpace(paymentReference))
            details.Add(new EmailDetailRow("Referans", paymentReference!));

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
                    ["PaymentAmount"] = FormatMoney(amount, currencyCode),
                    ["CurrencyCode"] = currencyCode,
                    ["PaymentReference"] = paymentReference
                },
                new EmailContent(Details: details)),
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
        var details = new List<EmailDetailRow>
        {
            new("Sipariş No", orderNumber),
            new("Gönderi No", shipmentNumber)
        };

        if (!string.IsNullOrWhiteSpace(carrierName))
            details.Add(new EmailDetailRow("Kargo Firması", carrierName!));

        if (!string.IsNullOrWhiteSpace(trackingNumber))
            details.Add(new EmailDetailRow("Takip No", trackingNumber!));

        if (!string.IsNullOrWhiteSpace(description))
            details.Add(new EmailDetailRow("Açıklama", description!));

        var content = new EmailContent(
            CallToAction: string.IsNullOrWhiteSpace(trackingUrl)
                ? null
                : new EmailCallToAction("Kargoyu Takip Et", trackingUrl!),
            Details: details);

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
                },
                content),
            cancellationToken);
    }

    private static string FormatMoney(decimal amount, string currencyCode)
    {
        return $"{amount:#,##0.00} {currencyCode}".Trim();
    }
}
