using MediatR;
using Order.Application.Orders.DTOs;

namespace Order.Application.Orders.Queries.GetStoreOrderById;

public sealed record GetStoreOrderByIdQuery(
    Guid StoreId,
    Guid OrderId) : IRequest<OrderDto?>;
