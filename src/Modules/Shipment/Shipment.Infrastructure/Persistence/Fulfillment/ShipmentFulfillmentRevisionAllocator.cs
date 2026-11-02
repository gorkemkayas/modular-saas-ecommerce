using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Shipment.Infrastructure.Persistence.Fulfillment;

internal static class ShipmentFulfillmentRevisionAllocator
{
    public static async Task<long> NextAsync(
        ShipmentDbContext dbContext, Guid storeId, Guid orderId, CancellationToken cancellationToken)
    {
        var transaction = dbContext.Database.CurrentTransaction
            ?? throw new InvalidOperationException("Fulfillment revisions require the Shipment transaction.");

        const string sql = """
            INSERT INTO "ShipmentFulfillmentSequences" ("StoreId", "OrderId", "LastRevision")
            VALUES (@storeId, @orderId, 1)
            ON CONFLICT ("StoreId", "OrderId") DO UPDATE
            SET "LastRevision" = "ShipmentFulfillmentSequences"."LastRevision" + 1
            RETURNING "LastRevision";
            """;

        await using var command = new NpgsqlCommand(sql,
            (NpgsqlConnection)dbContext.Database.GetDbConnection(),
            (NpgsqlTransaction)transaction.GetDbTransaction());
        command.Parameters.AddWithValue("storeId", storeId);
        command.Parameters.AddWithValue("orderId", orderId);
        return (long)(await command.ExecuteScalarAsync(cancellationToken))!;
    }
}
