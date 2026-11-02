using Order.Domain.Enums;
using Order.Domain.Exceptions;
using Order.Domain.Models;
using Order.Domain.ValueObjects;

namespace Order.Domain.UnitTests.Entities;

[TestClass]
public sealed class ShipmentFulfillmentTests
{
    [TestMethod]
    public void LatestSnapshot_CanArriveFirst_AndOlderMessagesCannotUndoIt()
    {
        var order = Create();
        var shipment = Guid.NewGuid();
        Assert.IsTrue(order.ApplyShipmentFulfillment(shipment, "SHP-1", ShipmentFulfillmentState.Delivered, 4));
        Assert.IsFalse(order.ApplyShipmentFulfillment(shipment, "SHP-1", ShipmentFulfillmentState.Draft, 1));
        Assert.IsFalse(order.ApplyShipmentFulfillment(shipment, "SHP-1", ShipmentFulfillmentState.Shipped, 3));
        Assert.IsFalse(order.ApplyShipmentFulfillment(shipment, "SHP-1", ShipmentFulfillmentState.Delivered, 4));
        Assert.AreEqual(FulfillmentStatus.Delivered, order.FulfillmentStatus);
        Assert.AreEqual(4L, order.LastFulfillmentRevision);
        Assert.AreEqual(OrderStatus.Confirmed, order.Status);
    }

    [TestMethod]
    public void ReplacementShipment_UsesOrderRevision_AndOldCancellationIsIgnored()
    {
        var order = Create();
        var old = Guid.NewGuid();
        var replacement = Guid.NewGuid();
        order.ApplyShipmentFulfillment(old, "A", ShipmentFulfillmentState.Draft, 1);
        order.ApplyShipmentFulfillment(replacement, "B", ShipmentFulfillmentState.Draft, 3);
        Assert.IsFalse(order.ApplyShipmentFulfillment(old, "A", ShipmentFulfillmentState.Cancelled, 2));
        Assert.AreEqual("B", order.ShipmentReference);
        Assert.AreEqual(replacement, order.LastFulfillmentShipmentId);
    }

    [TestMethod]
    public void Cancellation_ClearsReference_ButRetainsRevisionTombstone()
    {
        var order = Create();
        var shipment = Guid.NewGuid();
        order.ApplyShipmentFulfillment(shipment, "A", ShipmentFulfillmentState.Cancelled, 2);
        Assert.IsNull(order.ShipmentReference);
        Assert.AreEqual(shipment, order.LastFulfillmentShipmentId);
        Assert.AreEqual(2L, order.LastFulfillmentRevision);
        Assert.IsFalse(order.ApplyShipmentFulfillment(shipment, "A", ShipmentFulfillmentState.Draft, 1));
        Assert.ThrowsExactly<OrderDomainException>(() =>
            order.ApplyShipmentFulfillment(shipment, "A", ShipmentFulfillmentState.Shipped, 3));
    }

    [TestMethod]
    public void DeliveryException_RemainsShipped_AndTerminalStatesCannotRegress()
    {
        var order = Create();
        var shipment = Guid.NewGuid();
        order.ApplyShipmentFulfillment(shipment, "A", ShipmentFulfillmentState.DeliveryException, 3);
        Assert.AreEqual(FulfillmentStatus.Shipped, order.FulfillmentStatus);
        Assert.ThrowsExactly<OrderDomainException>(() =>
            order.ApplyShipmentFulfillment(shipment, "A", ShipmentFulfillmentState.Cancelled, 4));
        order.ApplyShipmentFulfillment(shipment, "A", ShipmentFulfillmentState.Delivered, 5);
        Assert.ThrowsExactly<OrderDomainException>(() =>
            order.ApplyShipmentFulfillment(shipment, "A", ShipmentFulfillmentState.Shipped, 6));
    }

    private static global::Order.Domain.Entities.Order Create() => global::Order.Domain.Entities.Order.Place(
        Guid.NewGuid(), OrderNumber.Create("ORD-1"),
        CustomerSnapshot.Create(Guid.NewGuid(), "test@example.com", "Test Customer", "+905550000000"),
        Address(), Address(), "TRY",
        [new OrderItemDraft(Guid.NewGuid(), null, "Product", null, "SKU", 1,
            OrderPriceSnapshot.Create(100m, "TRY", null, Guid.NewGuid(), Guid.NewGuid()))]);

    private static OrderAddressSnapshot Address() => OrderAddressSnapshot.Create("Home", "Customer",
        "+905550000000", "Turkey", "Istanbul", "Kadikoy", "Street 1", null, "34000");
}
