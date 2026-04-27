namespace Order.Application.Exceptions;

public sealed class OrderNotFoundException : ApplicationException
{
    public OrderNotFoundException(Guid orderId)
        : base("Order was not found.")
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}
