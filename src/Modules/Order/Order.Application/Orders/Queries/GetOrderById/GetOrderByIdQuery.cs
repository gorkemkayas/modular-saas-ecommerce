using MediatR;
using Order.Application.Orders.DTOs;

namespace Order.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(
    Guid StoreId,
    Guid ExternalUserId,
    Guid OrderId) : IRequest<OrderDto?>;
