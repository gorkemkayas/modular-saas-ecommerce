using BuildingBlocks.Application.Events;
using BuildingBlocks.Domain.Events;
using MediatR;
using Moq;
using Order.Application.Events;
using Order.Domain.Events;

namespace Order.Application.UnitTests.Events;

[TestClass]
public sealed class OrderDomainEventDispatcherTests
{
    [TestMethod]
    public async Task DispatchAsync_WhenOrderCancelledEventProvided_PublishesTypedNotification()
    {
        var publisher = new Mock<IPublisher>();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var domainEvent = new OrderCancelledDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-TEST-0001",
            "customer@example.com",
            "Jane Doe",
            "Customer changed mind");

        publisher
            .Setup(x => x.Publish(
                It.IsAny<DomainEventNotification<OrderCancelledDomainEvent>>(),
                cancellationToken))
            .Returns(Task.CompletedTask);

        var dispatcher = new OrderDomainEventDispatcher(publisher.Object);

        await dispatcher.DispatchAsync(
            new IDomainEvent[] { domainEvent },
            cancellationToken);

        publisher.Verify(
            x => x.Publish(
                It.Is<DomainEventNotification<OrderCancelledDomainEvent>>(
                    notification =>
                        ReferenceEquals(notification.DomainEvent, domainEvent)),
                cancellationToken),
            Times.Once);

        publisher.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task DispatchAsync_WhenEventTypeIsUnsupported_ThrowsInvalidOperationException()
    {
        var publisher = new Mock<IPublisher>();
        var dispatcher = new OrderDomainEventDispatcher(publisher.Object);
        var unsupportedEvent = new UnsupportedOrderDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow);

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => dispatcher.DispatchAsync(
                new IDomainEvent[] { unsupportedEvent },
                CancellationToken.None));

        StringAssert.Contains(
            exception.Message,
            typeof(UnsupportedOrderDomainEvent).FullName);

        publisher.VerifyNoOtherCalls();
    }

    private sealed record UnsupportedOrderDomainEvent(
        Guid Id,
        DateTime OccurredOnUtc) : IDomainEvent;
}