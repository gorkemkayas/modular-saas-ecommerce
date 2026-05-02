namespace Order.Contracts;

public sealed record OrderPaymentContextResult(
    Guid OrderId,
    Guid StoreId,
    Guid CustomerId,
    string OrderNumber,
    OrderStatusContract Status,
    OrderPaymentStatusContract PaymentStatus,
    OrderFulfillmentStatusContract FulfillmentStatus,
    decimal GrandTotalAmount,
    string CurrencyCode,
    string? ReservationReference,
    OrderPaymentCustomerResult Customer,
    OrderPaymentAddressResult BillingAddress,
    OrderPaymentAddressResult ShippingAddress,
    IReadOnlyCollection<OrderPaymentItemResult> Items);

public sealed record OrderPaymentCustomerResult(
    string Email,
    string FullName,
    string? PhoneNumber);

public sealed record OrderPaymentAddressResult(
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode);

public sealed record OrderPaymentItemResult(
    Guid ProductId,
    string ProductName,
    string? VariantName,
    string? Sku,
    int Quantity,
    decimal LineTotalAmount);
