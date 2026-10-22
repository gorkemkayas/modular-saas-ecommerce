using BuildingBlocks.Domain.Events;

namespace Order.Domain.Events;

public sealed record OrderCancelledDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid OrderId,
    Guid StoreId,
    Guid CustomerId,
    string OrderNumber,
    string RecipientEmail,
    string RecipientName,
    string? CancellationReason) : IDomainEvent;
