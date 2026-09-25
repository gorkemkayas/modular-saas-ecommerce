using BuildingBlocks.Application.Events;
using MediatR;
using Order.Application.Abstractions;
using Order.Contracts.IntegrationEvents;
using Order.Domain.Events;

namespace Order.Application.Events;

public sealed class OrderCancelledDomainEventHandler : INotificationHandler<DomainEventNotification<OrderCancelledDomainEvent>>
{
    private readonly IOrderOutboxWriter _orderOutboxWriter;

    public OrderCancelledDomainEventHandler(IOrderOutboxWriter orderOutboxWriter)
    {
        _orderOutboxWriter = orderOutboxWriter;
    }

    public Task Handle(
        DomainEventNotification<OrderCancelledDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(notification.DomainEvent);

        var domainEvent = notification.DomainEvent;

        var integrationEvent = new OrderCancelledIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            domainEvent.OrderId,
            domainEvent.StoreId,
            domainEvent.CustomerId,
            domainEvent.OrderNumber,
            domainEvent.RecipientEmail,
            domainEvent.RecipientName,
            domainEvent.CancellationReason);

        _orderOutboxWriter.Add(integrationEvent);

        return Task.CompletedTask;
    }
}
