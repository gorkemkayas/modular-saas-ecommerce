namespace Customer.Domain.Exceptions;

public class CustomerDomainException : Exception
{
    public CustomerDomainException(string message)
        : base(message)
    {
    }
}
