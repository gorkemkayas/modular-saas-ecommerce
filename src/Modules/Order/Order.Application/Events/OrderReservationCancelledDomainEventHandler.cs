using BuildingBlocks.Application.Events;
using MediatR;
using Order.Application.Abstractions;
using Order.Contracts.IntegrationEvents;
using Order.Domain.Events;

namespace Order.Application.Events;

public sealed class OrderReservationCancelledDomainEventHandler(IOrderOutboxWriter writer)
    : INotificationHandler<DomainEventNotification<OrderReservationCancelledDomainEvent>>
{
    public Task Handle(DomainEventNotification<OrderReservationCancelledDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.DomainEvent;
        writer.Add(new OrderReservationCancelledIntegrationEvent(e.Id, e.OccurredOnUtc,
            e.StoreId, e.OrderId, e.ReservationReference, e.CancellationReason));
        return Task.CompletedTask;
    }
}
