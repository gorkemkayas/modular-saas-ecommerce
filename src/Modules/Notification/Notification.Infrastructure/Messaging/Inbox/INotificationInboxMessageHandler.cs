using Notification.Infrastructure.Persistence.Inbox;

namespace Notification.Infrastructure.Messaging.Inbox;

public interface INotificationInboxMessageHandler
{
    string EventType { get; }

    Task HandleAsync(
        ClaimedInboxMessage message,
        CancellationToken cancellationToken = default);
}