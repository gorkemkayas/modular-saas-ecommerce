namespace Store.Domain.Exceptions
{
    public sealed class InvalidStoreNameException : DomainException
    {
        public InvalidStoreNameException()
            : base("Store name cannot be empty.")
        {
        }
    }


}
