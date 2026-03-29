namespace Catalog.Domain.Exceptions
{
    public sealed class InvalidProductPublishStateException : CatalogDomainException
    {
        public InvalidProductPublishStateException(string message) : base(message)
        {
        }
    }
}
