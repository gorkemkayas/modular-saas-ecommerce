using BuildingBlocks.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.RabbitMQ.Connections;

public sealed class RabbitMqConnectionProvider :
    IRabbitMqConnectionProvider
{
    private readonly IConnectionFactory _publishingConnectionFactory;
    private readonly IConnectionFactory _consumingConnectionFactory;
    private readonly SemaphoreSlim _connectionGate = new(1, 1);

    private IConnection? _publishingConnection;
    private IConnection? _consumingConnection;
    private bool _disposed;

    public RabbitMqConnectionProvider(
        IOptions<RabbitMqOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        RabbitMqOptions rabbitMqOptions = options.Value;

        _publishingConnectionFactory =
            CreateConnectionFactory(
        rabbitMqOptions,
        rabbitMqOptions.PublishingClientProvidedName);

        _consumingConnectionFactory =
            CreateConnectionFactory(
        rabbitMqOptions,
        rabbitMqOptions.ConsumingClientProvidedName);
    }

    public ValueTask<IConnection> GetConnectionAsync(
        RabbitMqConnectionRole role,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed),
            this);

        IConnection? connection =
            ReadConnection(role);

        if (connection is not null)
        {
            return ValueTask.FromResult(connection);
        }

        return CreateConnectionAsync(
            role,
            cancellationToken);
    }

    private async ValueTask<IConnection> CreateConnectionAsync(
        RabbitMqConnectionRole role,
        CancellationToken cancellationToken)
    {
        await _connectionGate.WaitAsync(
            cancellationToken);

        try
        {
            ObjectDisposedException.ThrowIf(
                Volatile.Read(ref _disposed),
                this);

            IConnection? existingConnection =
                ReadConnection(role);

            if (existingConnection is not null)
            {
                return existingConnection;
            }

            IConnectionFactory connectionFactory =
                GetConnectionFactory(role);

            IConnection connection = await connectionFactory
                .CreateConnectionAsync(
                    cancellationToken);

            WriteConnection(
                role,
                connection);

            return connection;
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    private IConnection? ReadConnection(
        RabbitMqConnectionRole role)
    {
        return role switch
        {
            RabbitMqConnectionRole.Publishing =>
                Volatile.Read(
                    ref _publishingConnection),

            RabbitMqConnectionRole.Consuming =>
                Volatile.Read(
                    ref _consumingConnection),

            _ => throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "Unknown RabbitMQ connection role.")
        };
    }

    private void WriteConnection(
        RabbitMqConnectionRole role,
        IConnection connection)
    {
        switch (role)
        {
            case RabbitMqConnectionRole.Publishing:
                Volatile.Write(
                    ref _publishingConnection,
                    connection);
                break;

            case RabbitMqConnectionRole.Consuming:
                Volatile.Write(
                    ref _consumingConnection,
                    connection);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(role),
                    role,
                    "Unknown RabbitMQ connection role.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connectionGate.WaitAsync();

        try
        {
            if (_disposed)
            {
                return;
            }

            Volatile.Write(
                ref _disposed,
                true);

            IConnection? publishingConnection =
                Interlocked.Exchange(
                    ref _publishingConnection,
                    null);

            IConnection? consumingConnection =
                Interlocked.Exchange(
                    ref _consumingConnection,
                    null);

            if (publishingConnection is not null)
            {
                await publishingConnection.DisposeAsync();
            }

            if (consumingConnection is not null)
            {
                await consumingConnection.DisposeAsync();
            }
        }
        finally
        {
            _connectionGate.Release();
        }
    }
    private static IConnectionFactory CreateConnectionFactory(
    RabbitMqOptions options,
    string clientProvidedName)
    {
        return new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost,
            ClientProvidedName = clientProvidedName,
            RequestedHeartbeat = options.RequestedHeartbeat,
            NetworkRecoveryInterval =
                options.NetworkRecoveryInterval,
            RequestedConnectionTimeout =
                options.ConnectionTimeout,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };
    }
    private IConnectionFactory GetConnectionFactory(
    RabbitMqConnectionRole role)
    {
        return role switch
        {
            RabbitMqConnectionRole.Publishing =>
                _publishingConnectionFactory,

            RabbitMqConnectionRole.Consuming =>
                _consumingConnectionFactory,

            _ => throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "Unknown RabbitMQ connection role.")
        };
    }
}
