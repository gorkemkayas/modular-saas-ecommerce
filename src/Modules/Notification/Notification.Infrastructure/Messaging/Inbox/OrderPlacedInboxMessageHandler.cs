using Notification.Application.Contracts;
using Notification.Contracts;
using Notification.Infrastructure.Persistence.Inbox;
using Order.Contracts.IntegrationEvents;

namespace Notification.Infrastructure.Messaging.Inbox;

public sealed class OrderPlacedInboxMessageHandler : INotificationInboxMessageHandler
{
    private readonly NotificationModuleApi _notifications;
    public OrderPlacedInboxMessageHandler(QueuedNotificationSender sender) => _notifications = new(sender);
    public string EventType => OrderPlacedIntegrationEvent.EventType;

    public async Task HandleAsync(ClaimedInboxMessage message, CancellationToken cancellationToken = default)
    {
        var e = NotificationInboxEventReader.Read<OrderPlacedIntegrationEvent>(message, EventType);
        if (e.Items is null || e.Items.Count == 0)
            throw new InvalidDataException("Order confirmation requires order items.");
        await _notifications.SendOrderPlacedAsync(new SendOrderPlacedNotificationRequest(
            e.StoreId, e.OrderId, e.CustomerId, e.OrderNumber, e.RecipientEmail, e.RecipientName,
            e.GrandTotalAmount, e.CurrencyCode, e.Items.Select(x => new NotificationLineItem(
                x.Name, x.Variant, x.Quantity, x.LineTotalAmount, x.ImageUrl)).ToArray(),
            e.SubtotalAmount, e.ShippingAmount), cancellationToken);
    }
}
