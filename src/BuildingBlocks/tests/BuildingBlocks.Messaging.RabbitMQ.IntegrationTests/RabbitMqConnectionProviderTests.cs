using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using RabbitMQ.Client;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace BuildingBlocks.Messaging.RabbitMQ.IntegrationTests;

[TestClass]
public sealed class RabbitMqConnectionProviderTests
{
    [TestMethod]
    public async Task GetConnectionAsync_WhenCalledConcurrently_ReturnsSameOpenConnection()
    {
        RabbitMqOptions options = RabbitMqTestContainer.CreateOptions(
            $"unused.{Guid.NewGuid():N}");

        await using var connectionProvider =
            new RabbitMqConnectionProvider(OptionsFactory.Create(options));

        Task<IConnection>[] connectionTasks = Enumerable
            .Range(0, 20)
            .Select(_ => connectionProvider.GetConnectionAsync().AsTask())
            .ToArray();

        IConnection[] connections = await Task.WhenAll(connectionTasks);

        IConnection expectedConnection = connections[0];

        Assert.IsTrue(expectedConnection.IsOpen);

        foreach (IConnection connection in connections)
        {
            Assert.AreSame(expectedConnection, connection);
        }
    }
}
