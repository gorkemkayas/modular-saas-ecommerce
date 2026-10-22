using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using RabbitMQ.Client;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace BuildingBlocks.Messaging.RabbitMQ.IntegrationTests;

[TestClass]
public sealed class RabbitMqConnectionProviderTests
{
    [TestMethod]
    public async Task GetConnectionAsync_WhenPublishingRoleIsCalledConcurrently_ReturnsSameOpenConnection()
    {
        RabbitMqOptions options = RabbitMqTestContainer.CreateOptions(
            $"unused.{Guid.NewGuid():N}");

        await using var connectionProvider =
            new RabbitMqConnectionProvider(OptionsFactory.Create(options));

        Task<IConnection>[] connectionTasks = Enumerable
            .Range(0, 20)
            .Select(_ => connectionProvider.GetConnectionAsync(
                RabbitMqConnectionRole.Publishing
            ).AsTask())
            .ToArray();

        IConnection[] connections = await Task.WhenAll(connectionTasks);

        IConnection expectedConnection = connections[0];

        Assert.IsTrue(expectedConnection.IsOpen);

        foreach (IConnection connection in connections)
        {
            Assert.AreSame(expectedConnection, connection);
        }
    }

    [TestMethod]
    public async Task GetConnectionAsync_WhenConsumingRoleIsCalledConcurrently_ReturnsSameOpenConnection()
    {
        RabbitMqOptions options =
            RabbitMqTestContainer.CreateOptions(
                $"unused.{Guid.NewGuid():N}");

        await using var connectionProvider =
            new RabbitMqConnectionProvider(
                OptionsFactory.Create(options));

        Task<IConnection>[] connectionTasks = Enumerable
            .Range(0, 20)
            .Select(_ => connectionProvider
                .GetConnectionAsync(
                    RabbitMqConnectionRole.Consuming)
                .AsTask())
            .ToArray();

        IConnection[] connections =
            await Task.WhenAll(connectionTasks);

        IConnection expectedConnection =
            connections[0];

        Assert.IsTrue(expectedConnection.IsOpen);

        foreach (IConnection connection in connections)
        {
            Assert.AreSame(
                expectedConnection,
                connection);
        }
    }

    [TestMethod]
    public async Task GetConnectionAsync_WhenDifferentRolesAreRequested_ReturnsDifferentOpenConnections()
    {
        RabbitMqOptions options =
            RabbitMqTestContainer.CreateOptions(
                $"unused.{Guid.NewGuid():N}");

        await using var connectionProvider =
            new RabbitMqConnectionProvider(
                OptionsFactory.Create(options));

        IConnection publishingConnection =
            await connectionProvider.GetConnectionAsync(
                RabbitMqConnectionRole.Publishing);

        IConnection consumingConnection =
            await connectionProvider.GetConnectionAsync(
                RabbitMqConnectionRole.Consuming);

        Assert.IsTrue(publishingConnection.IsOpen);
        Assert.IsTrue(consumingConnection.IsOpen);

        Assert.AreNotSame(
            publishingConnection,
            consumingConnection);
    }
}
