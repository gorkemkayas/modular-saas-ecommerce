using Notification.Domain.Enums;

namespace Notification.Contracts;

public sealed record SendOrderPlacedNotificationRequest(
    Guid StoreId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string RecipientEmail,
    string RecipientName,
    decimal GrandTotalAmount,
    string CurrencyCode,
    IReadOnlyCollection<NotificationLineItem> Items,
    decimal SubtotalAmount,
    decimal ShippingAmount,
    string Locale = "default");

public sealed record NotificationLineItem(
    string Name,
    string? Variant,
    int Quantity,
    decimal LineTotalAmount,
    string? ImageUrl = null);

public sealed record SendOrderCancelledNotificationRequest(
    Guid StoreId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string RecipientEmail,
    string RecipientName,
    string? CancellationReason,
    string Locale = "default");

public sealed record SendPaymentAuthorizedNotificationRequest(
    Guid StoreId,
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string RecipientEmail,
    string RecipientName,
    decimal Amount,
    string CurrencyCode,
    string? PaymentReference,
    string Locale = "default");

public sealed record SendPaymentCapturedNotificationRequest(
    Guid StoreId,
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string RecipientEmail,
    string RecipientName,
    decimal Amount,
    string CurrencyCode,
    string? PaymentReference,
    string Locale = "default");

public sealed record SendPaymentFailedNotificationRequest(
    Guid StoreId,
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string RecipientEmail,
    string RecipientName,
    decimal Amount,
    string CurrencyCode,
    string? FailureMessage,
    string Locale = "default");

public sealed record SendPaymentRefundedNotificationRequest(
    Guid StoreId,
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string RecipientEmail,
    string RecipientName,
    decimal RefundedAmount,
    string CurrencyCode,
    string Locale = "default");

public sealed record SendShipmentCreatedNotificationRequest(
    Guid StoreId,
    Guid ShipmentId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string ShipmentNumber,
    string RecipientEmail,
    string RecipientName,
    string? CarrierName,
    string? TrackingNumber,
    string? TrackingUrl,
    string Locale = "default");

public sealed record SendShipmentShippedNotificationRequest(
    Guid StoreId,
    Guid ShipmentId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string ShipmentNumber,
    string RecipientEmail,
    string RecipientName,
    string? CarrierName,
    string? TrackingNumber,
    string? TrackingUrl,
    string Locale = "default");

public sealed record SendShipmentDeliveredNotificationRequest(
    Guid StoreId,
    Guid ShipmentId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string ShipmentNumber,
    string RecipientEmail,
    string RecipientName,
    string? CarrierName,
    string? TrackingNumber,
    string? TrackingUrl,
    string Locale = "default");

public sealed record SendShipmentDeliveryExceptionNotificationRequest(
    Guid StoreId,
    Guid ShipmentId,
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    string ShipmentNumber,
    string RecipientEmail,
    string RecipientName,
    string? CarrierName,
    string? TrackingNumber,
    string? TrackingUrl,
    string Description,
    string Locale = "default");
