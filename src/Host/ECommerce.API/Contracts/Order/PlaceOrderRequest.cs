namespace ECommerce.API.Contracts.Order;

public sealed record PlaceOrderRequest(
    Guid ShippingAddressId,
    Guid? BillingAddressId,
    Guid ShippingCarrierId,
    string CurrencyCode,
    IReadOnlyCollection<PlaceOrderItemRequest> Items);

public sealed record PlaceOrderItemRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);
