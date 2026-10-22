using BuildingBlocks.Messaging.RabbitMQ.Options;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace Notification.Infrastructure.IntegrationTests;

internal static class RabbitMqTestContainer
{
    private const string Username = "integration-tests";
    private const string Password = "integration-tests";

    private static RabbitMqContainer? _container;

    public static RabbitMqOptions CreateOptions(string exchangeName)
    {
        RabbitMqContainer container = GetContainer();

        return new RabbitMqOptions
        {
            HostName = container.Hostname,
            Port = container.GetMappedPublicPort(5672),
            UserName = Username,
            Password = Password,
            VirtualHost = "/",
            PublishingClientProvidedName =
                "notification-tests-publishing",
            ConsumingClientProvidedName =
                "notification-tests-consuming",
            ExchangeName = exchangeName,
            RequestedHeartbeat = TimeSpan.FromSeconds(10),
            NetworkRecoveryInterval = TimeSpan.FromSeconds(1),
            ConnectionTimeout = TimeSpan.FromSeconds(5),
            PublishTimeout = TimeSpan.FromSeconds(5)
        };
    }

    public static Task<IConnection> CreateConnectionAsync(
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

        return connectionFactory.CreateConnectionAsync(
            cancellationToken);
    }

    public static async Task InitializeAsync()
    {
        _container =
            new RabbitMqBuilder("rabbitmq:4.2.9-alpine")
                .WithUsername(Username)
                .WithPassword(Password)
                .Build();

        await _container.StartAsync();
    }

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
