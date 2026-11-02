using System.Text.Json;
using BuildingBlocks.Messaging.Abstractions.Events;
using Notification.Infrastructure.Persistence.Inbox;

namespace Notification.Infrastructure.Messaging.Inbox;

internal static class NotificationInboxEventReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static T Read<T>(ClaimedInboxMessage message, string eventType) where T : IIntegrationEvent
    {
        if (!string.Equals(message.EventType, eventType, StringComparison.Ordinal))
            throw new InvalidDataException("Inbox handler received an unexpected event type.");
        try
        {
            var result = JsonSerializer.Deserialize<T>(message.Payload, JsonOptions)
                ?? throw new InvalidDataException("Integration event cannot be null.");
            if (result.Id != message.Id)
                throw new InvalidDataException("Integration event identity does not match the Inbox record.");
            return result;
        }
        catch (JsonException exception) { throw new InvalidDataException("Invalid integration event payload.", exception); }
    }
}
