using MediatR;
using Order.Application.Abstractions.Queries;
using Order.Application.Orders.DTOs;

namespace Order.Application.Orders.Queries.GetStoreOrderById;

public sealed class GetStoreOrderByIdQueryHandler : IRequestHandler<GetStoreOrderByIdQuery, OrderDto?>
{
    private readonly IOrderReadService _orderReadService;

    public GetStoreOrderByIdQueryHandler(IOrderReadService orderReadService)
    {
        _orderReadService = orderReadService;
    }

    public Task<OrderDto?> Handle(GetStoreOrderByIdQuery query, CancellationToken cancellationToken)
    {
        return _orderReadService.GetByIdAsync(query.StoreId, query.OrderId, cancellationToken);
    }
}
