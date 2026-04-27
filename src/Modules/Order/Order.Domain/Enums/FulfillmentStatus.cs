namespace Order.Domain.Enums;

public enum FulfillmentStatus
{
    Unfulfilled = 0,
    PartiallyFulfilled = 1,
    Fulfilled = 2,
    Shipped = 3,
    Delivered = 4,
    Returned = 5
}
