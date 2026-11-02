using BuildingBlocks.Domain.Events;

namespace Order.Domain.Events;

public sealed record OrderReservationCancelledDomainEvent(
    Guid Id, DateTime OccurredOnUtc, Guid StoreId, Guid OrderId,
    string ReservationReference, string? CancellationReason) : IDomainEvent;
