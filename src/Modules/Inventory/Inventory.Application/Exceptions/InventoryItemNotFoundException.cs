namespace Inventory.Application.Exceptions;

public sealed class InventoryItemNotFoundException : ApplicationException
{
    public InventoryItemNotFoundException(Guid inventoryItemId)
        : base($"Inventory item '{inventoryItemId}' was not found.")
    {
        InventoryItemId = inventoryItemId;
    }

    public Guid InventoryItemId { get; }
}
