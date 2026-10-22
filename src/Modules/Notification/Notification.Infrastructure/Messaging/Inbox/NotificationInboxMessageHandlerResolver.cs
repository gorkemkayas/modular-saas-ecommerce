namespace Notification.Infrastructure.Messaging.Inbox;

public sealed class NotificationInboxMessageHandlerResolver
{
    private readonly IReadOnlyDictionary<
        string,
        INotificationInboxMessageHandler> _handlers;

    public NotificationInboxMessageHandlerResolver(
        IEnumerable<INotificationInboxMessageHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        INotificationInboxMessageHandler[] handlerArray =
            handlers.ToArray();

        INotificationInboxMessageHandler? invalidHandler =
            handlerArray.FirstOrDefault(
                handler =>
                    string.IsNullOrWhiteSpace(
                        handler.EventType));

        if (invalidHandler is not null)
        {
            throw new InvalidOperationException(
                $"Inbox handler " +
                $"'{invalidHandler.GetType().FullName}' " +
                "has an empty event type.");
        }

        string? duplicateEventType =
            handlerArray
                .GroupBy(
                    handler => handler.EventType,
                    StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .FirstOrDefault();

        if (duplicateEventType is not null)
        {
            throw new InvalidOperationException(
                $"Multiple Notification Inbox handlers " +
                $"are registered for event type " +
                $"'{duplicateEventType}'.");
        }

        _handlers = handlerArray.ToDictionary(
            handler => handler.EventType,
            StringComparer.Ordinal);
    }

    public INotificationInboxMessageHandler Resolve(
        string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException(
                "Inbox event type is required.",
                nameof(eventType));
        }

        if (!_handlers.TryGetValue(
                eventType,
                out INotificationInboxMessageHandler? handler))
        {
            throw new UnsupportedNotificationInboxEventException(
                eventType);
        }

        return handler;
    }
}