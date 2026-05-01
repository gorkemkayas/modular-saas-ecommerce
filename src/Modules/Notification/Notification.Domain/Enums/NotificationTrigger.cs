namespace Notification.Domain.Enums;

public enum NotificationTrigger
{
    OrderPlaced = 1,
    OrderCancelled = 2,
    PaymentAuthorized = 3,
    PaymentCaptured = 4,
    PaymentFailed = 5,
    PaymentRefunded = 6,
    ShipmentCreated = 7,
    ShipmentShipped = 8,
    ShipmentDelivered = 9,
    ShipmentDeliveryException = 10
}
