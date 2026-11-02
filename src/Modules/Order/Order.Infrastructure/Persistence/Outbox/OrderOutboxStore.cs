using BuildingBlocks.Infrastructure.Messaging.Outbox;

namespace Order.Infrastructure.Persistence.Outbox;

public sealed class OrderOutboxStore(OrderDbContext context)
    : ModuleOutboxStore<OrderDbContext>(context), IOrderOutboxStore
{
}
