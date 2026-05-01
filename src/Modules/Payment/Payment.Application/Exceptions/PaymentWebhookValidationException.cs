namespace Payment.Application.Exceptions;

public sealed class PaymentWebhookValidationException : ApplicationException
{
    public PaymentWebhookValidationException(string message)
        : base(message)
    {
    }
}
