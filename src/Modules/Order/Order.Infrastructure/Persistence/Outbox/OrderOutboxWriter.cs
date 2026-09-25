using System.Text.Json;
using Order.Application.Abstractions;
using Order.Contracts.IntegrationEvents;

namespace Order.Infrastructure.Persistence.Outbox;

public sealed class OrderOutboxWriter : IOrderOutboxWriter
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly OrderDbContext _dbContext;

    public OrderOutboxWriter(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(IOrderIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var payload = JsonSerializer.Serialize(
            integrationEvent,
            integrationEvent.GetType(),
            JsonOptions);

        var outboxMessage = new OutboxMessage(
            integrationEvent.Id,
            integrationEvent.StoreId,
            integrationEvent.EventType,
            integrationEvent.EventType,
            payload,
            integrationEvent.OccurredOnUtc,
            DateTime.UtcNow);

        _dbContext.OutboxMessages.Add(outboxMessage);
    }
}