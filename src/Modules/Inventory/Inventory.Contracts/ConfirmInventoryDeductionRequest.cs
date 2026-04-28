namespace Inventory.Contracts;

public sealed record ConfirmInventoryDeductionRequest(
    Guid StoreId,
    string ReservationReference,
    string Reason);
