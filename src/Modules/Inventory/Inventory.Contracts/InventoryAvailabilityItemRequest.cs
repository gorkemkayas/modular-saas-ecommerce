namespace Inventory.Contracts;

public sealed record InventoryAvailabilityItemRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);
