using Order.Domain.Enums;

namespace Order.Contracts;

public sealed record OrderShipmentContextResult(
    Guid OrderId,
    Guid StoreId,
    Guid CustomerId,
    string OrderNumber,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    FulfillmentStatus FulfillmentStatus,
    string? ShipmentReference,
    OrderShipmentAddressResult ShippingAddress,
    IReadOnlyCollection<OrderShipmentItemResult> Items);

public sealed record OrderShipmentAddressResult(
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode);

public sealed record OrderShipmentItemResult(
    Guid OrderItemId,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string? Sku,
    int Quantity);
