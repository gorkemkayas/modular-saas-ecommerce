using BuildingBlocks.Domain.Events;

namespace Order.Domain.Events;

public sealed record OrderPlacedDomainEvent(
    Guid Id, DateTime OccurredOnUtc, Guid OrderId, Guid StoreId, Guid CustomerId,
    string OrderNumber, string RecipientEmail, string RecipientName, string CurrencyCode,
    decimal SubtotalAmount, decimal ShippingAmount, decimal GrandTotalAmount,
    IReadOnlyCollection<OrderPlacedLine> Items) : IDomainEvent;

public sealed record OrderPlacedLine(
    Guid ProductId, Guid? ProductVariantId, string Name, string? Variant,
    int Quantity, decimal LineTotalAmount, string? ImageUrl);
