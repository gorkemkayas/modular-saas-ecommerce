using Notification.Contracts;
using Shipment.Application.Integrations;

namespace Shipment.Infrastructure.Integrations.Notification;

public sealed class ShipmentNotificationService : IShipmentNotificationService
{
    private readonly INotificationModuleApi _notificationModuleApi;

    public ShipmentNotificationService(INotificationModuleApi notificationModuleApi)
    {
        _notificationModuleApi = notificationModuleApi;
    }

    public Task SendShipmentCreatedAsync(
        Guid storeId,
        Guid shipmentId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string shipmentNumber,
        string recipientEmail,
        string recipientName,
        string? carrierName,
        string? trackingNumber,
        string? trackingUrl,
        CancellationToken cancellationToken = default)
    {
        return _notificationModuleApi.SendShipmentCreatedAsync(
            new SendShipmentCreatedNotificationRequest(
                storeId,
                shipmentId,
                orderId,
                customerId,
                orderNumber,
                shipmentNumber,
                recipientEmail,
                recipientName,
                carrierName,
                trackingNumber,
                trackingUrl),
            cancellationToken);
    }

    public Task SendShipmentShippedAsync(
        Guid storeId,
        Guid shipmentId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string shipmentNumber,
        string recipientEmail,
        string recipientName,
        string? carrierName,
        string? trackingNumber,
        string? trackingUrl,
        CancellationToken cancellationToken = default)
    {
        return _notificationModuleApi.SendShipmentShippedAsync(
            new SendShipmentShippedNotificationRequest(
                storeId,
                shipmentId,
                orderId,
                customerId,
                orderNumber,
                shipmentNumber,
                recipientEmail,
                recipientName,
                carrierName,
                trackingNumber,
                trackingUrl),
            cancellationToken);
    }

    public Task SendShipmentDeliveredAsync(
        Guid storeId,
        Guid shipmentId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string shipmentNumber,
        string recipientEmail,
        string recipientName,
        string? carrierName,
        string? trackingNumber,
        string? trackingUrl,
        CancellationToken cancellationToken = default)
    {
        return _notificationModuleApi.SendShipmentDeliveredAsync(
            new SendShipmentDeliveredNotificationRequest(
                storeId,
                shipmentId,
                orderId,
                customerId,
                orderNumber,
                shipmentNumber,
                recipientEmail,
                recipientName,
                carrierName,
                trackingNumber,
                trackingUrl),
            cancellationToken);
    }

    public Task SendShipmentDeliveryExceptionAsync(
        Guid storeId,
        Guid shipmentId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string shipmentNumber,
        string recipientEmail,
        string recipientName,
        string? carrierName,
        string? trackingNumber,
        string? trackingUrl,
        string description,
        CancellationToken cancellationToken = default)
    {
        return _notificationModuleApi.SendShipmentDeliveryExceptionAsync(
            new SendShipmentDeliveryExceptionNotificationRequest(
                storeId,
                shipmentId,
                orderId,
                customerId,
                orderNumber,
                shipmentNumber,
                recipientEmail,
                recipientName,
                carrierName,
                trackingNumber,
                trackingUrl,
                description),
            cancellationToken);
    }
}
