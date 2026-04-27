using MediatR;
using Order.Application.Common.Models;
using Order.Application.Orders.DTOs;

namespace Order.Application.Orders.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery(
    Guid StoreId,
    Guid ExternalUserId,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<OrderSummaryDto>>;
