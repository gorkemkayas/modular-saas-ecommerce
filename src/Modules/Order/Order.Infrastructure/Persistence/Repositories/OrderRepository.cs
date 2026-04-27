using Microsoft.EntityFrameworkCore;
using Order.Domain.Repositories;
using Order.Infrastructure.Persistence;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        return _context.Orders.AddAsync(order, cancellationToken).AsTask();
    }

    public Task<OrderEntity?> GetByIdAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default)
    {
        return _context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == orderId, cancellationToken);
    }
}
