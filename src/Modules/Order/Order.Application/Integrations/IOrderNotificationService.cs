namespace Order.Application.Integrations;

public interface IOrderNotificationService
{
    Task SendOrderPlacedAsync(
        Guid storeId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string recipientEmail,
        string recipientName,
        decimal grandTotalAmount,
        string currencyCode,
        CancellationToken cancellationToken = default);

    Task SendOrderCancelledAsync(
        Guid storeId,
        Guid orderId,
        Guid customerId,
        string orderNumber,
        string recipientEmail,
        string recipientName,
        string? cancellationReason,
        CancellationToken cancellationToken = default);
}
