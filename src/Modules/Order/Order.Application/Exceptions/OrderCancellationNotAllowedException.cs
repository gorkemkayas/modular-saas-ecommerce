namespace Order.Application.Exceptions;

public sealed class OrderCancellationNotAllowedException()
    : ApplicationException("Close the pending or authorized payment before cancelling this order. Captured/refunded orders cannot be cancelled.");
