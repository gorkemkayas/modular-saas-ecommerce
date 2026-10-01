using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.RabbitMQ.Connections;

public interface IRabbitMqConnectionProvider : IAsyncDisposable
{
    ValueTask<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default);
}