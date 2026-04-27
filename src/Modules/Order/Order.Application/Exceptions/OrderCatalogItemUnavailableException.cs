namespace Order.Application.Exceptions;

public sealed class OrderCatalogItemUnavailableException : ApplicationException
{
    public OrderCatalogItemUnavailableException(Guid productId, Guid? productVariantId)
        : base("The requested sellable item is unavailable for ordering.")
    {
        ProductId = productId;
        ProductVariantId = productVariantId;
    }

    public Guid ProductId { get; }
    public Guid? ProductVariantId { get; }
}
