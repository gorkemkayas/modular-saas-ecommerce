namespace Store.Application.Exceptions
{
    public sealed class DuplicateStoreSlugException : ApplicationException
    {
        public string Slug { get; }

        public DuplicateStoreSlugException(string slug)
            : base($"A store with slug '{slug}' already exists.")
        {
            Slug = slug;
        }
    }
}
