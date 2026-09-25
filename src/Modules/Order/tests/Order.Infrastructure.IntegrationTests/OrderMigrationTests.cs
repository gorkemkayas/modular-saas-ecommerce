using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure.IntegrationTests;

[TestClass]
public sealed class OrderMigrationTests
{
    [TestMethod]
    public async Task MigrateAsync_CreatesOutboxMessagesTable()
    {
        await using var serviceProvider =
            OrderIntegrationTestHost.CreateServiceProvider();

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

        await dbContext.Database.MigrateAsync();

        var connection = dbContext.Database.GetDbConnection();

        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name = 'OutboxMessages'
            );
            """;

        var result = await command.ExecuteScalarAsync();

        var tableExists = Assert.IsInstanceOfType<bool>(result);

        Assert.IsTrue(tableExists);
    }
}