namespace Catalog.Domain.Exceptions
{
    public sealed class DuplicateVariantCombinationException : CatalogDomainException
    {
        public DuplicateVariantCombinationException()
            : base("The same variant combination already exists for this product.")
        {
        }
    }
}
