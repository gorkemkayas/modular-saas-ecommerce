namespace Shipment.Application.Integrations;

public sealed record OrderShipmentContext(
    Guid OrderId,
    Guid StoreId,
    Guid CustomerId,
    string OrderNumber,
    string CustomerEmail,
    string CustomerFullName,
    OrderShipmentStatus Status,
    OrderShipmentPaymentStatus PaymentStatus,
    OrderShipmentFulfillmentStatus FulfillmentStatus,
    string? ShipmentReference,
    OrderShipmentAddress ShippingAddress,
    IReadOnlyCollection<OrderShipmentItem> Items);

public sealed record OrderShipmentAddress(
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode);

public sealed record OrderShipmentItem(
    Guid OrderItemId,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string? Sku,
    int Quantity);
