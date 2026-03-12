namespace Store.Domain.Exceptions
{
    public sealed class InvalidStoreStatusException : DomainException
    {
        public InvalidStoreStatusException(string message) : base(message)
        {
        }

        public static InvalidStoreStatusException CannotPublish()
            => new("Only active stores can be published.");

        public static InvalidStoreStatusException CannotActivate()
            => new("Archived store cannot be activated.");
    }


}
