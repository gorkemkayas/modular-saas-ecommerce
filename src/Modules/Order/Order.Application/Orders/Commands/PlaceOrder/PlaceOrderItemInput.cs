namespace Order.Application.Orders.Commands.PlaceOrder;

public sealed record PlaceOrderItemInput(
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);
