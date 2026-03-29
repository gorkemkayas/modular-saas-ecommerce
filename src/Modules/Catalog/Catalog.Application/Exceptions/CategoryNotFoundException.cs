namespace Catalog.Application.Exceptions
{
    public sealed class CategoryNotFoundException : ApplicationException
    {
        public CategoryNotFoundException(Guid categoryId)
            : base($"Category with id '{categoryId}' was not found.")
        {
            CategoryId = categoryId;
        }

        public Guid CategoryId { get; }
    }
}
