namespace Inventory.Contracts;

public sealed record ReserveInventoryRequest(
    Guid StoreId,
    Guid OrderId,
    string ReservationReference,
    IReadOnlyCollection<InventoryReservationItemRequest> Items);
