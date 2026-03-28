namespace Catalog.Application.Exceptions
{
    public sealed class DuplicateBrandSlugException : ApplicationException
    {
        public DuplicateBrandSlugException(string slug)
            : base($"Brand slug '{slug}' is already in use.")
        {
            Slug = slug;
        }

        public string Slug { get; }
    }
}
