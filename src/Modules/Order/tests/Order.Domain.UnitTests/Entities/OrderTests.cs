using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.Exceptions;
using Order.Domain.Models;
using Order.Domain.ValueObjects;

namespace Order.Domain.UnitTests.Entities;

[TestClass]
public sealed class OrderTests
{
    [TestMethod]
    public void Place_WithValidInputs_CreatesConfirmedOrder()
    {
        var order = Order.Domain.Entities.Order.Place(
            Guid.NewGuid(),
            OrderNumber.Create("ORD-TEST-0001"),
            CustomerSnapshot.Create(Guid.NewGuid(), "customer@example.com", "Jane Doe", "+90 555 000 00 00"),
            CreateAddress("Billing"),
            CreateAddress("Shipping"),
            "TRY",
            new[]
            {
                new OrderItemDraft(
                    Guid.NewGuid(),
                    null,
                    "Phone",
                    null,
                    "SKU-1",
                    2,
                    OrderPriceSnapshot.Create(100m, "TRY", 120m, Guid.NewGuid(), Guid.NewGuid()))
            });

        Assert.AreEqual(OrderStatus.Confirmed, order.Status);
        Assert.AreEqual(PaymentStatus.Pending, order.PaymentStatus);
        Assert.AreEqual(FulfillmentStatus.Unfulfilled, order.FulfillmentStatus);
        Assert.AreEqual(200m, order.Totals.SubtotalAmount);
        Assert.AreEqual(200m, order.Totals.GrandTotalAmount);
        Assert.HasCount(1, order.Items);
    }

    [TestMethod]
    public void Cancel_WhenOrderAlreadyShipped_ThrowsOrderDomainException()
    {
        var order = CreateOrder();
        order.MarkShipped("SHIP-1");

        Assert.ThrowsExactly<OrderDomainException>(() => order.Cancel("Customer changed mind"));

        Assert.HasCount(0, order.DomainEvents);
    }

    [TestMethod]
    public void Cancel_WhenPaymentCaptured_ThrowsOrderDomainException()
    {
        var order = CreateOrder();
        order.MarkPaymentCaptured("PAY-1");

        Assert.ThrowsExactly<OrderDomainException>(() => order.Cancel("Customer changed mind"));

        Assert.HasCount(0, order.DomainEvents);
    }

    [TestMethod]
    public void Cancel_WhenPaymentRefunded_ThrowsOrderDomainException()
    {
        var order = CreateOrder();
        order.MarkPaymentCaptured("PAY-1");
        order.MarkPaymentRefunded("PAY-1");

        Assert.ThrowsExactly<OrderDomainException>(() => order.Cancel("Customer changed mind"));

        Assert.HasCount(0, order.DomainEvents);
    }

    [TestMethod]
    public void Cancel_WhenOrderCanBeCancelled_RaisesOrderCancelledDomainEvent()
    {
        var order = CreateOrder();

        order.Cancel("Customer changed mind");

        Assert.HasCount(1, order.DomainEvents);

        var domainEvent = order.DomainEvents
            .OfType<OrderCancelledDomainEvent>()
            .Single();

        Assert.AreNotEqual(Guid.Empty, domainEvent.Id);
        Assert.AreEqual(DateTimeKind.Utc, domainEvent.OccurredOnUtc.Kind);
        Assert.AreEqual(order.CancelledAtUtc, domainEvent.OccurredOnUtc);
        Assert.AreEqual(order.Id, domainEvent.OrderId);
        Assert.AreEqual(order.StoreId, domainEvent.StoreId);
        Assert.AreEqual(order.CustomerId, domainEvent.CustomerId);
        Assert.AreEqual(order.OrderNumber.Value, domainEvent.OrderNumber);
        Assert.AreEqual(order.CustomerSnapshot.Email, domainEvent.RecipientEmail);
        Assert.AreEqual(order.CustomerSnapshot.FullName, domainEvent.RecipientName);
        Assert.AreEqual(order.CancellationReason, domainEvent.CancellationReason);
    }

    [TestMethod]
    public void Cancel_WhenOrderAlreadyCancelled_DoesNotRaiseAnotherDomainEvent()
    {
        var order = CreateOrder();
        order.Cancel("Customer changed mind");

        var firstEvent = order.DomainEvents
            .OfType<OrderCancelledDomainEvent>()
            .Single();

        order.Cancel("A different reason");

        Assert.HasCount(1, order.DomainEvents);

        var remainingEvent = order.DomainEvents
            .OfType<OrderCancelledDomainEvent>()
            .Single();

        Assert.AreEqual(firstEvent.Id, remainingEvent.Id);
        Assert.AreEqual("Customer changed mind", order.CancellationReason);
    }

    [TestMethod]
    public void ClearDomainEvents_WhenOrderHasDomainEvents_RemovesAllEvents()
    {
        var order = CreateOrder();
        order.Cancel("Customer changed mind");

        order.ClearDomainEvents();

        Assert.HasCount(0, order.DomainEvents);
    }

    private static Order.Domain.Entities.Order CreateOrder()
    {
        return Order.Domain.Entities.Order.Place(
            Guid.NewGuid(),
            OrderNumber.Create("ORD-TEST-0002"),
            CustomerSnapshot.Create(Guid.NewGuid(), "customer@example.com", "Jane Doe", "+90 555 000 00 00"),
            CreateAddress("Billing"),
            CreateAddress("Shipping"),
            "TRY",
            new[]
            {
                new OrderItemDraft(
                    Guid.NewGuid(),
                    null,
                    "Phone",
                    null,
                    "SKU-1",
                    1,
                    OrderPriceSnapshot.Create(100m, "TRY", null, Guid.NewGuid(), Guid.NewGuid()))
            });
    }

    private static OrderAddressSnapshot CreateAddress(string title)
    {
        return OrderAddressSnapshot.Create(
            title,
            "Jane Doe",
            "+90 555 000 00 00",
            "Turkey",
            "Istanbul",
            "Kadikoy",
            "Street 1",
            null,
            "34000");
    }
}
