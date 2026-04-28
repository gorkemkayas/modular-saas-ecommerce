namespace Inventory.Contracts;

public sealed record InventoryReservationItemRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);
