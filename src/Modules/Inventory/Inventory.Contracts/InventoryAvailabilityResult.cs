namespace Inventory.Contracts;

public sealed record InventoryAvailabilityResult(
    bool IsAvailable,
    IReadOnlyCollection<InventoryAvailabilityItemResult> Items);
