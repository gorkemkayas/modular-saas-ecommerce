using Payment.Domain.Enums;

namespace Payment.Application.Integrations;

public sealed record PaymentGatewayAuthorizeRequest(
    Guid PaymentId,
    Guid StoreId,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    decimal Amount,
    string CurrencyCode,
    PaymentMethodType MethodType,
    string IdempotencyKey,
    string ClientIpAddress,
    PaymentGatewayCustomer Customer,
    PaymentGatewayAddress BillingAddress,
    PaymentGatewayAddress ShippingAddress,
    IReadOnlyCollection<PaymentGatewayBasketItem> Items,
    Guid? ProviderAccountId = null);

public sealed record PaymentGatewayCustomer(
    string Email,
    string FullName,
    string? PhoneNumber);

public sealed record PaymentGatewayAddress(
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode);

public sealed record PaymentGatewayBasketItem(
    Guid ProductId,
    string ProductName,
    string? VariantName,
    string? Sku,
    int Quantity,
    decimal LineTotalAmount);
