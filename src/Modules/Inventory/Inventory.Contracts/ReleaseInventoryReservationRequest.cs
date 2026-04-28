namespace Inventory.Contracts;

public sealed record ReleaseInventoryReservationRequest(
    Guid StoreId,
    string ReservationReference,
    string Reason);
