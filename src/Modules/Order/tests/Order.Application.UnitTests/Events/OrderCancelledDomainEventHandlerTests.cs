using System;
using BuildingBlocks.Application.Events;
using Moq;
using Order.Application.Abstractions;
using Order.Application.Events;
using Order.Contracts.IntegrationEvents;
using Order.Domain.Events;

namespace Order.Application.UnitTests.Events;

[TestClass]
public sealed class OrderCancelledDomainEventHandlerTests
{
    [TestMethod]
    public async Task Handle_WhenOrderCancelled_StagesMatchingIntegrationEvent()
    {
        var eventId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var occurredOnUtc = new DateTime(
            2026,
            9,
            25,
            10,
            30,
            0,
            DateTimeKind.Utc);

        var domainEvent = new OrderCancelledDomainEvent(
            eventId,
            occurredOnUtc,
            orderId,
            storeId,
            customerId,
            "ORD-TEST-0001",
            "customer@example.com",
            "Jane Doe",
            "Customer changed mind");

        IOrderIntegrationEvent? capturedEvent = null;

        var outboxWriter = new Mock<IOrderOutboxWriter>();

        outboxWriter
            .Setup(x => x.Add(
                It.IsAny<IOrderIntegrationEvent>()))
            .Callback<IOrderIntegrationEvent>(
                integrationEvent =>
                    capturedEvent = integrationEvent);

        var handler = new OrderCancelledDomainEventHandler(
            outboxWriter.Object);

        var notification =
            new DomainEventNotification<OrderCancelledDomainEvent>(
                domainEvent);

        await handler.Handle(
            notification,
            CancellationToken.None);

        outboxWriter.Verify(
            x => x.Add(It.IsAny<IOrderIntegrationEvent>()),
            Times.Once);

        outboxWriter.VerifyNoOtherCalls();

        Assert.IsNotNull(capturedEvent);
        Assert.IsInstanceOfType<
            OrderCancelledIntegrationEvent>(capturedEvent);

        var integrationEvent =
            (OrderCancelledIntegrationEvent)capturedEvent;

        Assert.AreEqual(eventId, integrationEvent.Id);
        Assert.AreEqual(occurredOnUtc, integrationEvent.OccurredOnUtc);
        Assert.AreEqual(orderId, integrationEvent.OrderId);
        Assert.AreEqual(storeId, integrationEvent.StoreId);
        Assert.AreEqual(customerId, integrationEvent.CustomerId);
        Assert.AreEqual("ORD-TEST-0001", integrationEvent.OrderNumber);
        Assert.AreEqual(
            "customer@example.com",
            integrationEvent.RecipientEmail);
        Assert.AreEqual("Jane Doe", integrationEvent.RecipientName);
        Assert.AreEqual(
            "Customer changed mind",
            integrationEvent.CancellationReason);

        Assert.AreEqual(
            OrderCancelledIntegrationEvent.EventType,
            capturedEvent.EventType);
    }
}