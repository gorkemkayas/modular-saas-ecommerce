namespace Inventory.Domain.Exceptions;

public sealed class InventoryDomainException : Exception
{
    public InventoryDomainException(string message)
        : base(message)
    {
    }
}
