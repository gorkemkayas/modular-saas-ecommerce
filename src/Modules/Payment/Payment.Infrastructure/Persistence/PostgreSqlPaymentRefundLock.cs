using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Npgsql;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Infrastructure.Options;

namespace Payment.Infrastructure.Persistence;

public sealed class PostgreSqlPaymentRefundLock(IOptions<PaymentDatabaseOptions> options) : IPaymentRefundLock
{
    public async Task<IAsyncDisposable> AcquireAsync(
        Guid storeId,
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        // A dedicated session serializes refund commands/callbacks across API instances.
        // No database transaction is held during the provider call. Closing this
        // nonpooled session releases its advisory lock, including on process failure.
        var connectionString = new NpgsqlConnectionStringBuilder(options.Value.ConnectionString)
        {
            Pooling = false
        };
        var connection = new NpgsqlConnection(connectionString.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"payment-refund/{storeId:D}/{paymentId:D}"));
            long key = BinaryPrimitives.ReadInt64LittleEndian(hash);
            await using var command = new NpgsqlCommand("SELECT pg_try_advisory_lock(@key)", connection);
            command.Parameters.AddWithValue("key", key);
            if (await command.ExecuteScalarAsync(cancellationToken) is not true)
                throw new PaymentOperationInProgressException();
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
