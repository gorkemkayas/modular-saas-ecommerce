namespace Store.Domain.Exceptions
{
    public sealed class ArchivedStoreException : DomainException
    {
        public ArchivedStoreException()
            : base("Archived store cannot be modified.")
        {
        }

        public ArchivedStoreException(string operation)
            : base($"Archived store cannot be {operation}.")
        {
        }
    }


}
