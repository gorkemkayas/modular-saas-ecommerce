using MediatR;
using Order.Application.Abstractions.Queries;
using Order.Application.Integrations;
using Order.Application.Orders.DTOs;

namespace Order.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderReadService _orderReadService;
    private readonly IOrderCustomerContextService _customerContextService;

    public GetOrderByIdQueryHandler(
        IOrderReadService orderReadService,
        IOrderCustomerContextService customerContextService)
    {
        _orderReadService = orderReadService;
        _customerContextService = customerContextService;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var customerIdentity = await _customerContextService.GetCustomerIdentityAsync(
            query.StoreId,
            query.ExternalUserId,
            cancellationToken);

        if (customerIdentity is null)
            return null;

        return await _orderReadService.GetByCustomerAsync(
            query.StoreId,
            customerIdentity.CustomerId,
            query.OrderId,
            cancellationToken);
    }
}
