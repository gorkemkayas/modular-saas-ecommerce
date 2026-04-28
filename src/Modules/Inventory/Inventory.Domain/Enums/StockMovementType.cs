namespace Inventory.Domain.Enums;

public enum StockMovementType
{
    Created = 1,
    StockAdded = 2,
    StockAdjusted = 3,
    Reserved = 4,
    ReservationReleased = 5,
    Deducted = 6,
    ReorderThresholdChanged = 7
}
