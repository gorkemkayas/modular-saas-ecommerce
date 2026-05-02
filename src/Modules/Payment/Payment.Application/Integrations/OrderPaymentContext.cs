namespace Payment.Application.Integrations;

public sealed record OrderPaymentContext(
    Guid OrderId,
    Guid StoreId,
    Guid CustomerId,
    string OrderNumber,
    OrderLifecycleStatus Status,
    OrderPaymentLifecycleStatus PaymentStatus,
    OrderFulfillmentLifecycleStatus FulfillmentStatus,
    decimal GrandTotalAmount,
    string CurrencyCode,
    string? ReservationReference,
    OrderPaymentCustomer Customer,
    OrderPaymentAddress BillingAddress,
    OrderPaymentAddress ShippingAddress,
    IReadOnlyCollection<OrderPaymentItem> Items);

public sealed record OrderPaymentCustomer(
    string Email,
    string FullName,
    string? PhoneNumber);

public sealed record OrderPaymentAddress(
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode);

public sealed record OrderPaymentItem(
    Guid ProductId,
    string ProductName,
    string? VariantName,
    string? Sku,
    int Quantity,
    decimal LineTotalAmount);

public enum OrderLifecycleStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}

public enum OrderPaymentLifecycleStatus
{
    Pending = 0,
    Authorized = 1,
    Captured = 2,
    Failed = 3,
    Refunded = 4
}

public enum OrderFulfillmentLifecycleStatus
{
    Unfulfilled = 0,
    PartiallyFulfilled = 1,
    Fulfilled = 2,
    Shipped = 3,
    Delivered = 4,
    Returned = 5
}
