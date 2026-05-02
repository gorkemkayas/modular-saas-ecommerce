namespace Order.Contracts;

public enum OrderStatusContract
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}

public enum OrderPaymentStatusContract
{
    Pending = 0,
    Authorized = 1,
    Captured = 2,
    Failed = 3,
    Refunded = 4
}

public enum OrderFulfillmentStatusContract
{
    Unfulfilled = 0,
    PartiallyFulfilled = 1,
    Fulfilled = 2,
    Shipped = 3,
    Delivered = 4,
    Returned = 5
}
