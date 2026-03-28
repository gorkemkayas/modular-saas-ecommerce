namespace Catalog.Application.Exceptions
{
    public sealed class DuplicateProductSlugException : ApplicationException
    {
        public DuplicateProductSlugException(string slug)
            : base($"Product slug '{slug}' is already in use.")
        {
            Slug = slug;
        }

        public string Slug { get; }
    }
}
