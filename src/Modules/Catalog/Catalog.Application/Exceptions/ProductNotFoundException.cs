namespace Catalog.Application.Exceptions
{
    public sealed class ProductNotFoundException : ApplicationException
    {
        public ProductNotFoundException(Guid productId)
            : base($"Product with id '{productId}' was not found.")
        {
            ProductId = productId;
        }

        public Guid ProductId { get; }
    }
}
