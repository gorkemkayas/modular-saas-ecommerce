namespace Payment.Application.Exceptions;

public sealed class UnauthorizedPaymentAccessException : ApplicationException
{
    public UnauthorizedPaymentAccessException()
        : base("Payment access is not authorized.")
    {
    }
}
