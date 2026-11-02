namespace Order.Contracts;

public sealed class OrderOperationInProgressException()
    : Exception("Another payment or cancellation operation is in progress for this order. Retry later.");
