namespace Notification.Infrastructure.Persistence.Inbox;

public enum InboxMessageStatus
{
    Pending = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4
}
