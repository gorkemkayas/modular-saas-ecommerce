namespace Store.Domain.Exceptions
{
    public sealed class InvalidTenantException : DomainException
    {
        public InvalidTenantException()
            : base("TenantId cannot be empty.")
        {
        }
    }


}
