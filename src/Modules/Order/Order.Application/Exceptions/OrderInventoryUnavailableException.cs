namespace Order.Application.Exceptions;

public sealed class OrderInventoryUnavailableException : ApplicationException
{
    public OrderInventoryUnavailableException(string message)
        : base(message)
    {
    }
}
