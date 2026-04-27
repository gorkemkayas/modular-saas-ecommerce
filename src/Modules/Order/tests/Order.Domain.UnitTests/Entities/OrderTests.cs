using Order.Domain.Entities;
using Order.Domain.Enums;
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
