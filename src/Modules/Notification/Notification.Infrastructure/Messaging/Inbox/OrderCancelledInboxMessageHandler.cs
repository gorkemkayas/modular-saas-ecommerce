using Notification.Application.Contracts;
using Notification.Contracts;
using Notification.Infrastructure.Persistence.Inbox;
using Order.Contracts.IntegrationEvents;

namespace Notification.Infrastructure.Messaging.Inbox;

public sealed class OrderCancelledInboxMessageHandler : INotificationInboxMessageHandler
{
    private readonly NotificationModuleApi _notifications;

    public OrderCancelledInboxMessageHandler(QueuedNotificationSender sender) => _notifications = new(sender);

    public string EventType => OrderCancelledIntegrationEvent.EventType;

    public async Task HandleAsync(ClaimedInboxMessage message, CancellationToken cancellationToken = default)
    {
        var e = NotificationInboxEventReader.Read<OrderCancelledIntegrationEvent>(message, EventType);
        await _notifications.SendOrderCancelledAsync(new SendOrderCancelledNotificationRequest(
            e.StoreId, e.OrderId, e.CustomerId, e.OrderNumber, e.RecipientEmail,
            e.RecipientName, e.CancellationReason), cancellationToken);
    }
}
