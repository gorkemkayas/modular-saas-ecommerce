namespace Order.Contracts.IntegrationEvents;

public sealed record OrderCancelledIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid OrderId,
    Guid StoreId,
    Guid CustomerId,
    string OrderNumber,
    string RecipientEmail,
    string RecipientName,
    string? CancellationReason)
{
    public const string EventType = "order.order-cancelled.v1";
}