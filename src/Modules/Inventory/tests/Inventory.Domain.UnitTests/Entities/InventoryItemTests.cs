using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Exceptions;

namespace Inventory.Domain.UnitTests.Entities;

[TestClass]
public sealed class InventoryItemTests
{
    [TestMethod]
    public void Reserve_WhenStockIsAvailable_IncreasesReservedQuantity()
    {
        var inventoryItem = InventoryItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "SKU-1",
            "Product",
            10,
            2);

        inventoryItem.Reserve(Guid.NewGuid(), "RES-1", 3);

        Assert.AreEqual(10, inventoryItem.OnHandQuantity);
        Assert.AreEqual(3, inventoryItem.ReservedQuantity);
        Assert.AreEqual(7, inventoryItem.AvailableQuantity);
        Assert.AreEqual(InventoryReservationStatus.Active, inventoryItem.Reservations.Single().Status);
    }

    [TestMethod]
    public void ConfirmReservation_WhenReservationExists_DeductsStockAndMarksConfirmed()
    {
        var inventoryItem = InventoryItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "SKU-1",
            "Product",
            10,
            2);

        inventoryItem.Reserve(Guid.NewGuid(), "RES-1", 4);
        inventoryItem.ConfirmReservation("RES-1", "Order paid.");

        Assert.AreEqual(6, inventoryItem.OnHandQuantity);
        Assert.AreEqual(0, inventoryItem.ReservedQuantity);
        Assert.AreEqual(6, inventoryItem.AvailableQuantity);
        Assert.AreEqual(InventoryReservationStatus.Confirmed, inventoryItem.Reservations.Single().Status);
        Assert.AreEqual(StockMovementType.Deducted, inventoryItem.Movements.Last().Type);
    }

    [TestMethod]
    public void AdjustStock_WhenNewOnHandWouldBeLessThanReserved_Throws()
    {
        var inventoryItem = InventoryItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "SKU-1",
            "Product",
            10,
            null);

        inventoryItem.Reserve(Guid.NewGuid(), "RES-1", 5);

        Assert.ThrowsExactly<InventoryDomainException>(() =>
            inventoryItem.AdjustStock(4, "Cycle count"));
    }
}
