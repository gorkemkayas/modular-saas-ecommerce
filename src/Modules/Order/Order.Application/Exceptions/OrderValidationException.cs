namespace Order.Application.Exceptions;

public sealed class OrderValidationException : ApplicationException
{
    public OrderValidationException(string message)
        : base(message)
    {
    }
}
