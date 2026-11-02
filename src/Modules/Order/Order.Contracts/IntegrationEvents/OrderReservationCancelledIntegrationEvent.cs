using BuildingBlocks.Messaging.Abstractions.Events;

namespace Order.Contracts.IntegrationEvents;

public sealed record OrderReservationCancelledIntegrationEvent(
    Guid Id, DateTime OccurredOnUtc, Guid StoreId, Guid OrderId,
    string ReservationReference, string? CancellationReason) : IOrderIntegrationEvent
{
    public const string EventType = "order.reservation-cancelled.v1";
    string IIntegrationEvent.EventType => EventType;
}
