using MediatR;
using Order.Application.Abstractions.Queries;
using Order.Application.Common.Models;
using Order.Application.Integrations;
using Order.Application.Orders.DTOs;

namespace Order.Application.Orders.Queries.GetMyOrders;

public sealed class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IOrderReadService _orderReadService;
    private readonly IOrderCustomerContextService _customerContextService;

    public GetMyOrdersQueryHandler(
        IOrderReadService orderReadService,
        IOrderCustomerContextService customerContextService)
    {
        _orderReadService = orderReadService;
        _customerContextService = customerContextService;
    }

    public async Task<PagedResult<OrderSummaryDto>> Handle(GetMyOrdersQuery query, CancellationToken cancellationToken)
    {
        var customerIdentity = await _customerContextService.GetCustomerIdentityAsync(
            query.StoreId,
            query.ExternalUserId,
            cancellationToken);

        if (customerIdentity is null)
            return new PagedResult<OrderSummaryDto>(Array.Empty<OrderSummaryDto>(), query.PageNumber, query.PageSize, 0);

        return await _orderReadService.SearchByCustomerAsync(
            query.StoreId,
            customerIdentity.CustomerId,
            query.PageNumber,
            query.PageSize,
            cancellationToken);
    }
}
