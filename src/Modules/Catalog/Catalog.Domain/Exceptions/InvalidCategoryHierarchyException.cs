namespace Catalog.Domain.Exceptions
{
    public sealed class InvalidCategoryHierarchyException : CatalogDomainException
    {
        public InvalidCategoryHierarchyException(string message) : base(message)
        {
        }
    }
}
