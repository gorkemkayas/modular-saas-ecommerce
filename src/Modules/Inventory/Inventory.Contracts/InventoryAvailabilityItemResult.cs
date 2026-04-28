namespace Inventory.Contracts;

public sealed record InventoryAvailabilityItemResult(
    Guid ProductId,
    Guid? ProductVariantId,
    bool IsAvailable,
    int RequestedQuantity,
    int AvailableQuantity);
