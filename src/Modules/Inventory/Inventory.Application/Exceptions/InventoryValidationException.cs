namespace Inventory.Application.Exceptions;

public sealed class InventoryValidationException : ApplicationException
{
    public InventoryValidationException(string message)
        : base(message)
    {
    }
}
