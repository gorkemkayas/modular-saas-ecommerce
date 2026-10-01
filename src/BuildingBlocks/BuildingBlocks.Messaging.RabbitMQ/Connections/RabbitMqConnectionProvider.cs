using BuildingBlocks.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.RabbitMQ.Connections;

public sealed class RabbitMqConnectionProvider : IRabbitMqConnectionProvider
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly SemaphoreSlim _connectionGate = new(1, 1);

    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnectionProvider(
        IOptions<RabbitMqOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        RabbitMqOptions rabbitMqOptions = options.Value;

        _connectionFactory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.HostName,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.UserName,
            Password = rabbitMqOptions.Password,
            VirtualHost = rabbitMqOptions.VirtualHost,
            ClientProvidedName =
                rabbitMqOptions.ClientProvidedName,
            RequestedHeartbeat =
                rabbitMqOptions.RequestedHeartbeat,
            NetworkRecoveryInterval =
                rabbitMqOptions.NetworkRecoveryInterval,
            RequestedConnectionTimeout =
                rabbitMqOptions.ConnectionTimeout,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };
    }
    public ValueTask<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed),
            this);

        IConnection? connection = Volatile.Read(ref _connection);

        if (connection is not null)
        {
            return ValueTask.FromResult(connection);
        }

        return CreateConnectionAsync(cancellationToken);
    }

    private async ValueTask<IConnection> CreateConnectionAsync(
        CancellationToken cancellationToken)
    {
        await _connectionGate.WaitAsync(cancellationToken);

        try
        {
            ObjectDisposedException.ThrowIf(
                Volatile.Read(ref _disposed),
                this);

            IConnection? existingConnection = Volatile.Read(ref _connection);

            if (existingConnection is not null)
            {
                return existingConnection;
            }

            IConnection connection =
                await _connectionFactory.CreateConnectionAsync(cancellationToken);

            Volatile.Write(ref _connection, connection);

            return connection;
        }
        finally
        {
            _connectionGate.Release();
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

            Volatile.Write(ref _disposed, true);

            IConnection? connection = Interlocked.Exchange(ref _connection, null);

            if (connection is not null)
            {
                await connection.DisposeAsync();
            }
        }
        finally
        {
            _connectionGate.Release();
        }

    }

}
