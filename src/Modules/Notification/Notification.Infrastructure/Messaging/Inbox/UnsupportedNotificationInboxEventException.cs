namespace Notification.Infrastructure.Messaging.Inbox;

public sealed class UnsupportedNotificationInboxEventException
    : Exception
{
    public UnsupportedNotificationInboxEventException(
        string eventType)
        : base(
            $"Notification Inbox event type " +
            $"'{eventType}' is not supported.")
    {
        EventType = eventType;
    }

    public string EventType { get; }
}