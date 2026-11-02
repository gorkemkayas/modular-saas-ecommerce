using Notification.Application.Contracts;
using Notification.Contracts;
using Notification.Infrastructure.Persistence.Inbox;
using Shipment.Contracts.IntegrationEvents;

namespace Notification.Infrastructure.Messaging.Inbox;

public sealed class ShipmentStatusChangedInboxMessageHandler : INotificationInboxMessageHandler
{
    private readonly NotificationModuleApi _notifications;
    private readonly NotificationRecipientResolver _recipients;
    public ShipmentStatusChangedInboxMessageHandler(QueuedNotificationSender sender, NotificationRecipientResolver recipients)
    {
        _notifications = new(sender);
        _recipients = recipients;
    }
    public string EventType => ShipmentStatusChangedIntegrationEvent.EventType;

    public async Task HandleAsync(ClaimedInboxMessage message, CancellationToken cancellationToken = default)
    {
        var e = NotificationInboxEventReader.Read<ShipmentStatusChangedIntegrationEvent>(message, EventType);
        if (!Enum.IsDefined(e.Kind))
            throw new InvalidDataException("Unsupported shipment event kind.");
        var recipient = await _recipients.ResolveAsync(e.StoreId, e.OrderId, e.CustomerId,
            e.RecipientEmail, e.RecipientName, cancellationToken);
        switch (e.Kind)
        {
            case ShipmentEventKind.Created:
                await _notifications.SendShipmentCreatedAsync(new(e.StoreId, e.ShipmentId, e.OrderId, recipient.CustomerId,
                    e.OrderNumber, e.ShipmentNumber, recipient.Email, recipient.FullName, e.CarrierName,
                    e.TrackingNumber, e.TrackingUrl), cancellationToken);
                break;
            case ShipmentEventKind.Shipped:
                await _notifications.SendShipmentShippedAsync(new(e.StoreId, e.ShipmentId, e.OrderId, recipient.CustomerId,
                    e.OrderNumber, e.ShipmentNumber, recipient.Email, recipient.FullName, e.CarrierName,
                    e.TrackingNumber, e.TrackingUrl), cancellationToken);
                break;
            case ShipmentEventKind.Delivered:
                await _notifications.SendShipmentDeliveredAsync(new(e.StoreId, e.ShipmentId, e.OrderId, recipient.CustomerId,
                    e.OrderNumber, e.ShipmentNumber, recipient.Email, recipient.FullName, e.CarrierName,
                    e.TrackingNumber, e.TrackingUrl), cancellationToken);
                break;
            case ShipmentEventKind.DeliveryException:
                await _notifications.SendShipmentDeliveryExceptionAsync(new(e.StoreId, e.ShipmentId, e.OrderId, recipient.CustomerId,
                    e.OrderNumber, e.ShipmentNumber, recipient.Email, recipient.FullName, e.CarrierName,
                    e.TrackingNumber, e.TrackingUrl, e.Description ?? "Delivery exception"), cancellationToken);
                break;
            default: throw new InvalidDataException("Unsupported shipment event kind.");
        }
    }
}
