namespace Inventory.Contracts;

public sealed record CheckInventoryAvailabilityRequest(
    Guid StoreId,
    IReadOnlyCollection<InventoryAvailabilityItemRequest> Items);
