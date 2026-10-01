using BuildingBlocks.Messaging.RabbitMQ.Options;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace BuildingBlocks.Messaging.RabbitMQ.IntegrationTests;

[TestClass]
public sealed class RabbitMqTestContainer
{
    private const string Username = "integration-tests";
    private const string Password = "integration-tests";

    private static RabbitMqContainer? _container;

    internal static RabbitMqOptions CreateOptions(string exchangeName)
    {
        RabbitMqContainer container = GetContainer();

        return new RabbitMqOptions
        {
            HostName = container.Hostname,
            Port = container.GetMappedPublicPort(5672),
            UserName = Username,
            Password = Password,
            VirtualHost = "/",
            ClientProvidedName = "rabbitmq-publisher-integration-tests",
            ExchangeName = exchangeName,
            RequestedHeartbeat = TimeSpan.FromSeconds(10),
            NetworkRecoveryInterval = TimeSpan.FromSeconds(1),
            ConnectionTimeout = TimeSpan.FromSeconds(5),
            PublishTimeout = TimeSpan.FromSeconds(5)
        };
    }

    internal static Task<IConnection> CreateConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        RabbitMqContainer container = GetContainer();

        var connectionFactory = new ConnectionFactory
        {
            HostName = container.Hostname,
            Port = container.GetMappedPublicPort(5672),
            UserName = Username,
            Password = Password,
            VirtualHost = "/"
        };

        return connectionFactory.CreateConnectionAsync(cancellationToken);
    }

    [AssemblyInitialize]
    public static async Task InitializeAsync(TestContext _)
    {
        _container =
            new RabbitMqBuilder("rabbitmq:4.2.9-alpine")
                .WithUsername(Username)
                .WithPassword(Password)
                .Build();

        await _container.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task CleanupAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    private static RabbitMqContainer GetContainer()
    {
        return _container
            ?? throw new InvalidOperationException(
                "The RabbitMQ test container has not been initialized.");
    }
}
