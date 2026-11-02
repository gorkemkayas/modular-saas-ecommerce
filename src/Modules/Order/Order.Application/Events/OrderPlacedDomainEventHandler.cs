using BuildingBlocks.Application.Events;
using MediatR;
using Order.Application.Abstractions;
using Order.Contracts.IntegrationEvents;
using Order.Domain.Events;

namespace Order.Application.Events;

public sealed class OrderPlacedDomainEventHandler
    : INotificationHandler<DomainEventNotification<OrderPlacedDomainEvent>>
{
    private readonly IOrderOutboxWriter _writer;
    public OrderPlacedDomainEventHandler(IOrderOutboxWriter writer) => _writer = writer;

    public Task Handle(DomainEventNotification<OrderPlacedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var e = notification.DomainEvent;
        _writer.Add(new OrderPlacedIntegrationEvent(
            e.Id, e.OccurredOnUtc, e.OrderId, e.StoreId, e.CustomerId, e.OrderNumber,
            e.RecipientEmail, e.RecipientName, e.CurrencyCode, e.SubtotalAmount,
            e.ShippingAmount, e.GrandTotalAmount,
            e.Items.Select(x => new Order.Contracts.IntegrationEvents.OrderPlacedLine(
                x.Name, x.Variant, x.Quantity, x.LineTotalAmount, x.ImageUrl)).ToArray()));
        return Task.CompletedTask;
    }
}
