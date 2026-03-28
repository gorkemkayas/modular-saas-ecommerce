namespace Catalog.Application.Exceptions
{
    public sealed class CatalogValidationException : ApplicationException
    {
        public CatalogValidationException(string message)
            : base(message)
        {
        }
    }
}
