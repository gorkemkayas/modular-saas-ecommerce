namespace Catalog.Application.Exceptions
{
    public sealed class DuplicateProductSkuException : ApplicationException
    {
        public DuplicateProductSkuException(string sku)
            : base($"Product SKU '{sku}' is already in use.")
        {
            Sku = sku;
        }

        public string Sku { get; }
    }
}
