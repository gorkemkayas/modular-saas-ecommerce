namespace Order.Contracts;

public sealed record OrderShipmentContextResult(
    Guid OrderId,
    Guid StoreId,
    Guid CustomerId,
    string OrderNumber,
    string CustomerEmail,
    string CustomerFullName,
    OrderStatusContract Status,
    OrderPaymentStatusContract PaymentStatus,
    OrderFulfillmentStatusContract FulfillmentStatus,
    string? ShipmentReference,
    OrderShipmentAddressResult ShippingAddress,
    OrderShipmentCarrierResult? ShippingCarrier,
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

public sealed record OrderShipmentCarrierResult(
    Guid CarrierId,
    string Code,
    string Name,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl);
