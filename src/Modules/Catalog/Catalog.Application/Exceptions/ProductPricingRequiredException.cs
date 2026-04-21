namespace Catalog.Application.Exceptions;

public sealed class ProductPricingRequiredException : ApplicationException
{
    public ProductPricingRequiredException(Guid productId)
        : base("Product cannot be published before required prices are defined.")
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
