namespace Payment.Application.Exceptions;

public sealed class PaymentProviderAccountNotConfiguredException : ApplicationException
{
    public PaymentProviderAccountNotConfiguredException(string message)
        : base(message)
    {
    }
}
