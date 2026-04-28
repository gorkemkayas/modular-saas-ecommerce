namespace Inventory.Contracts;

public sealed record InventoryReservationItemResult(
    Guid InventoryItemId,
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);
