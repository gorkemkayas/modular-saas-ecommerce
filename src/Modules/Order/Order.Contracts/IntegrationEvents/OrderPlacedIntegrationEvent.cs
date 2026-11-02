using BuildingBlocks.Messaging.Abstractions.Events;

namespace Order.Contracts.IntegrationEvents;

public sealed record OrderPlacedIntegrationEvent(
    Guid Id, DateTime OccurredOnUtc, Guid OrderId, Guid StoreId, Guid CustomerId,
    string OrderNumber, string RecipientEmail, string RecipientName, string CurrencyCode,
    decimal SubtotalAmount, decimal ShippingAmount, decimal GrandTotalAmount,
    IReadOnlyCollection<OrderPlacedLine> Items) : IOrderIntegrationEvent
{
    public const string EventType = "order.order-placed.v1";
    string IIntegrationEvent.EventType => EventType;
}

public sealed record OrderPlacedLine(
    string Name, string? Variant, int Quantity, decimal LineTotalAmount, string? ImageUrl = null);
