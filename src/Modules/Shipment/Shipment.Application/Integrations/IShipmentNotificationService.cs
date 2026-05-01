namespace Shipment.Application.Integrations;

public interface IShipmentNotificationService
{
    Task SendShipmentCreatedAsync(
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
        CancellationToken cancellationToken = default);

    Task SendShipmentShippedAsync(
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
        CancellationToken cancellationToken = default);

    Task SendShipmentDeliveredAsync(
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
        CancellationToken cancellationToken = default);

    Task SendShipmentDeliveryExceptionAsync(
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
        CancellationToken cancellationToken = default);
}
