using Notification.Contracts;
using Order.Application.Integrations;

namespace Order.Infrastructure.Integrations.Notification;

public sealed class OrderNotificationService : IOrderNotificationService
{
    private readonly INotificationModuleApi _notificationModuleApi;

    public OrderNotificationService(INotificationModuleApi notificationModuleApi)
    {
        _notificationModuleApi = notificationModuleApi;
    }

    public Task SendOrderPlacedAsync(
        Guid storeId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string recipientEmail,
        string recipientName,
        decimal grandTotalAmount,
        string currencyCode,
        CancellationToken cancellationToken = default)
    {
        return _notificationModuleApi.SendOrderPlacedAsync(
            new SendOrderPlacedNotificationRequest(
                storeId,
                orderId,
                customerId,
                orderNumber,
                recipientEmail,
                recipientName,
                grandTotalAmount,
                currencyCode),
            cancellationToken);
    }

    public Task SendOrderCancelledAsync(
        Guid storeId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string recipientEmail,
        string recipientName,
        string? cancellationReason,
        CancellationToken cancellationToken = default)
    {
        return _notificationModuleApi.SendOrderCancelledAsync(
            new SendOrderCancelledNotificationRequest(
                storeId,
                orderId,
                customerId,
                orderNumber,
                recipientEmail,
                recipientName,
                cancellationReason),
            cancellationToken);
    }
}
