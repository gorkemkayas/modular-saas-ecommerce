namespace Payment.Application.Exceptions;

public sealed class PaymentValidationException : ApplicationException
{
    public PaymentValidationException(string message)
        : base(message)
    {
    }
}
