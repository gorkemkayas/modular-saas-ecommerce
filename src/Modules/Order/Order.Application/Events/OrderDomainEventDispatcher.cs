using BuildingBlocks.Application.Events;
using BuildingBlocks.Domain.Events;
using MediatR;
using Order.Domain.Events;

namespace Order.Application.Events;

public sealed class OrderDomainEventDispatcher : IOrderDomainEventDispatcher
{
    private readonly IPublisher _publisher;
    public OrderDomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            switch (domainEvent)
            {
                case OrderCancelledDomainEvent orderCancelled:
                    await _publisher.Publish(
                        new DomainEventNotification<OrderCancelledDomainEvent>(
                            orderCancelled),
                        cancellationToken);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported Order domain event type " +
                        $"'{domainEvent.GetType().FullName}'.");
            }
        }
    }
}
