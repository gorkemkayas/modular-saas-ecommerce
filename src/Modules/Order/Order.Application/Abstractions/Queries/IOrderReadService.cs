using Order.Application.Common.Models;
using Order.Application.Orders.DTOs;

namespace Order.Application.Abstractions.Queries;

public interface IOrderReadService
{
    Task<PagedResult<OrderSummaryDto>> SearchByCustomerAsync(
        Guid storeId,
        Guid customerId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<OrderDto?> GetByCustomerAsync(
        Guid storeId,
        Guid customerId,
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<OrderDto?> GetByIdAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken = default);
}
