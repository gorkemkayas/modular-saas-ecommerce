namespace Catalog.Application.Exceptions
{
    public sealed class DuplicateCategorySlugException : ApplicationException
    {
        public DuplicateCategorySlugException(string slug)
            : base($"Category slug '{slug}' is already in use.")
        {
            Slug = slug;
        }

        public string Slug { get; }
    }
}
