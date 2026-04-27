using OrderEntity = Order.Domain.Entities.Order;

namespace Order.Domain.Repositories;

public interface IOrderRepository
{
    Task AddAsync(OrderEntity order, CancellationToken cancellationToken = default);
    Task<OrderEntity?> GetByIdAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);
}
