using MediatR;

namespace Order.Application.Orders.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(
    Guid StoreId,
    Guid ExternalUserId,
    Guid ShippingAddressId,
    Guid? BillingAddressId,
    Guid ShippingCarrierId,
    string CurrencyCode,
    IReadOnlyCollection<PlaceOrderItemInput> Items) : IRequest<Guid>;
