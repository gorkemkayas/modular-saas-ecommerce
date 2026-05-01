namespace Payment.Application.Exceptions;

public sealed class PaymentNotFoundException : ApplicationException
{
    public PaymentNotFoundException(Guid paymentId)
        : base($"Payment '{paymentId}' was not found.")
    {
        PaymentId = paymentId;
    }

    public Guid PaymentId { get; }
}
