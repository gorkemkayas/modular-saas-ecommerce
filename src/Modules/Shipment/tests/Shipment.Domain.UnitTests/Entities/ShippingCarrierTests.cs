using Shipment.Domain.Entities;
using Shipment.Domain.Exceptions;

namespace Shipment.Domain.UnitTests.Entities;

[TestClass]
public sealed class ShippingCarrierTests
{
    [TestMethod]
    public void Create_WithValidInputs_NormalizesCodeAndActivatesCarrier()
    {
        var carrier = ShippingCarrier.Create(
            Guid.NewGuid(),
            " YURTICI ",
            "Yurtici Kargo",
            null,
            null,
            null,
            10);

        Assert.AreEqual("yurtici", carrier.Code);
        Assert.AreEqual("Yurtici Kargo", carrier.Name);
        Assert.IsTrue(carrier.IsActive);
        Assert.AreEqual(10, carrier.SortOrder);
    }

    [TestMethod]
    public void Create_WithWhitespaceCode_ThrowsShipmentDomainException()
    {
        Assert.ThrowsExactly<ShipmentDomainException>(() =>
            ShippingCarrier.Create(
                Guid.NewGuid(),
                "bad code",
                "Carrier",
                null,
                null,
                null,
                0));
    }
}
