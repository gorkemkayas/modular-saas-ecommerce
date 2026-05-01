namespace Notification.Contracts;

public interface INotificationModuleApi
{
    Task<Guid> SendOrderPlacedAsync(
        SendOrderPlacedNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> SendOrderCancelledAsync(
        SendOrderCancelledNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> SendPaymentAuthorizedAsync(
        SendPaymentAuthorizedNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> SendPaymentCapturedAsync(
        SendPaymentCapturedNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> SendPaymentFailedAsync(
        SendPaymentFailedNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> SendPaymentRefundedAsync(
        SendPaymentRefundedNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> SendShipmentCreatedAsync(
        SendShipmentCreatedNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> SendShipmentShippedAsync(
        SendShipmentShippedNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> SendShipmentDeliveredAsync(
        SendShipmentDeliveredNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> SendShipmentDeliveryExceptionAsync(
        SendShipmentDeliveryExceptionNotificationRequest request,
        CancellationToken cancellationToken = default);
}
