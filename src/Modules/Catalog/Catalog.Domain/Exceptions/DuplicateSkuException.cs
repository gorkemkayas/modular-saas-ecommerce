namespace Catalog.Domain.Exceptions
{
    public sealed class DuplicateSkuException : CatalogDomainException
    {
        public DuplicateSkuException(string sku)
            : base($"The SKU '{sku}' already exists in this product.")
        {
        }
    }
}
