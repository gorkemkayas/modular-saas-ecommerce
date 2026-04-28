namespace Inventory.Application.Exceptions;

public sealed class DuplicateInventoryItemException : ApplicationException
{
    public DuplicateInventoryItemException(Guid storeId, Guid productId, Guid? productVariantId)
        : base("Inventory item already exists for the given sellable item.")
    {
        StoreId = storeId;
        ProductId = productId;
        ProductVariantId = productVariantId;
    }

    public Guid StoreId { get; }
    public Guid ProductId { get; }
    public Guid? ProductVariantId { get; }
}
