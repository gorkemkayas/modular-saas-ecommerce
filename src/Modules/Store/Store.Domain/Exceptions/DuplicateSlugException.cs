namespace Store.Domain.Exceptions
{
    public sealed class DuplicateSlugException : DomainException
    {
        public DuplicateSlugException()
            : base("New slug cannot be the same as current slug.")
        {
        }
    }


}
