namespace Notification.Infrastructure.Persistence.Dispatch;

public enum NotificationDispatchDeliveryStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}
