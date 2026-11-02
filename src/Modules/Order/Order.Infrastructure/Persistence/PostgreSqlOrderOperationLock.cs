using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Npgsql;
using Order.Contracts;
using Order.Infrastructure.Options;

namespace Order.Infrastructure.Persistence;

public sealed class PostgreSqlOrderOperationLock(IOptions<OrderDatabaseOptions> options) : IOrderOperationLock
{
    public async Task<IAsyncDisposable> AcquireAsync(Guid storeId, Guid orderId,
        CancellationToken cancellationToken = default)
    {
        if (storeId == Guid.Empty || orderId == Guid.Empty)
            throw new ArgumentException("Store and order identities are required.");
        // Session ownership spans the provider call without holding a database transaction.
        // A nonpooled connection releases the lock on dispose or process failure.
        var settings = new NpgsqlConnectionStringBuilder(options.Value.ConnectionString) { Pooling = false };
        var connection = new NpgsqlConnection(settings.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            long key = BinaryPrimitives.ReadInt64LittleEndian(SHA256.HashData(
                Encoding.UTF8.GetBytes($"order-operation/{storeId:D}/{orderId:D}")));
            await using var command = new NpgsqlCommand("SELECT pg_try_advisory_lock(@key)", connection);
            command.Parameters.AddWithValue("key", key);
            if (await command.ExecuteScalarAsync(cancellationToken) is not true)
                throw new OrderOperationInProgressException();
            return connection;
        }
        catch { await connection.DisposeAsync(); throw; }
    }
}
