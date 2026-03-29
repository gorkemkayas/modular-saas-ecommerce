namespace Catalog.Domain.Exceptions
{
    public sealed class InvalidAttributeUsageException : CatalogDomainException
    {
        public InvalidAttributeUsageException(string message) : base(message)
        {
        }
    }
}
