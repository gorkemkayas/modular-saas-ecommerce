namespace Inventory.Contracts;

public sealed record InventoryReservationResult(
    string ReservationReference,
    IReadOnlyCollection<InventoryReservationItemResult> Items);
