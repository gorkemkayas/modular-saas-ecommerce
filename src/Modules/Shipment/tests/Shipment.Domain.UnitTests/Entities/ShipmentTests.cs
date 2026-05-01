using Shipment.Domain.Entities;
using Shipment.Domain.Enums;
using Shipment.Domain.Exceptions;
using Shipment.Domain.ValueObjects;

namespace Shipment.Domain.UnitTests.Entities;

[TestClass]
public sealed class ShipmentTests
{
    [TestMethod]
    public void MarkShipped_WithoutPackage_ThrowsShipmentDomainException()
    {
        var shipment = CreateShipment();

        Assert.ThrowsExactly<ShipmentDomainException>(() => shipment.MarkShipped());
    }

    [TestMethod]
    public void MarkDelivered_AfterShipping_MarksDelivered()
    {
        var shipment = CreateShipment();
        shipment.AddPackage("PKG-01", "TRK-1", null, null, null);
        shipment.MarkReadyForDispatch();
        shipment.MarkShipped();

        shipment.MarkDelivered();

        Assert.AreEqual(ShipmentStatus.Delivered, shipment.Status);
        Assert.IsNotNull(shipment.DeliveredAtUtc);
    }

    private static Shipment.Domain.Entities.Shipment CreateShipment()
    {
        return Shipment.Domain.Entities.Shipment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1001",
            "SHP-1001",
            "Jane Doe",
            "+90 555 000 00 00",
            ShipmentAddress.Create(
                "Jane Doe",
                "+90 555 000 00 00",
                "Turkey",
                "Istanbul",
                "Kadikoy",
                "Street 1",
                null,
                "34000"),
            new[]
            {
                new ShipmentLineDraft(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    null,
                    "Phone",
                    null,
                    "SKU-1",
                    1)
            });
    }
}
