namespace Payment.Application.Integrations;

public sealed record OrderPaymentContext(
    Guid OrderId,
    Guid StoreId,
    Guid CustomerId,
    string OrderNumber,
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
