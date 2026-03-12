namespace Store.Application.Exceptions
{
    public sealed class StoreNotFoundBySlugException : ApplicationException
    {
        public string Slug { get; }

        public StoreNotFoundBySlugException(string slug)
            : base($"Store with slug '{slug}' not found.")
        {
            Slug = slug;
        }
    }
}
