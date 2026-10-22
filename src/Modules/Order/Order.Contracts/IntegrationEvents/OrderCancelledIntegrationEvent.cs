using BuildingBlocks.Messaging.Abstractions.Events;

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
    string? CancellationReason) : IOrderIntegrationEvent
{
    public const string EventType = "order.order-cancelled.v1";

    string IIntegrationEvent.EventType => OrderCancelledIntegrationEvent.EventType;
}