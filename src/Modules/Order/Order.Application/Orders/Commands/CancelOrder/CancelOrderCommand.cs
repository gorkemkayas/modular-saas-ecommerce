using MediatR;

namespace Order.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId,
    string? Reason) : IRequest;
