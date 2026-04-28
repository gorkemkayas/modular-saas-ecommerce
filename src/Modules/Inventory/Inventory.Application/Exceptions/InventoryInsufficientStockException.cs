namespace Inventory.Application.Exceptions;

public sealed class InventoryInsufficientStockException : ApplicationException
{
    public InventoryInsufficientStockException(Guid productId, Guid? productVariantId, int requestedQuantity)
        : base("Insufficient stock for the requested inventory item.")
    {
        ProductId = productId;
        ProductVariantId = productVariantId;
        RequestedQuantity = requestedQuantity;
    }

    public Guid ProductId { get; }
    public Guid? ProductVariantId { get; }
    public int RequestedQuantity { get; }
}
