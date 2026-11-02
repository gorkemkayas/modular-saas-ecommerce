using BuildingBlocks.Infrastructure.Messaging.Outbox;

namespace Order.Infrastructure.Persistence.Outbox;

public interface IOrderOutboxStore : IModuleOutboxStore<OrderDbContext>
{
}
