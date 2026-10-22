using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.RabbitMQ.Connections;

public interface IRabbitMqConnectionProvider : IAsyncDisposable
{
    ValueTask<IConnection> GetConnectionAsync(
        RabbitMqConnectionRole role,
        CancellationToken cancellationToken = default);
}