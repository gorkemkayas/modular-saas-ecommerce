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
        IReadOnlyCollection<OrderNotificationLineItem> items,
        decimal subtotalAmount,
        decimal shippingAmount,
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

public sealed record OrderNotificationLineItem(
    string Name,
    string? Variant,
    int Quantity,
    decimal LineTotalAmount,
    string? ImageUrl = null);
