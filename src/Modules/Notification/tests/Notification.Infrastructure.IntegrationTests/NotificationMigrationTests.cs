using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class NotificationMigrationTests
{
    [TestMethod]
    public async Task MigrateAsync_DeliveryModelMatchesLatestSnapshot()
    {
        await using var serviceProvider = NotificationIntegrationTestHost.CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        await dbContext.Database.MigrateAsync();

        Assert.IsFalse(dbContext.Database.HasPendingModelChanges());
        _ = await dbContext.NotificationDispatchDeliveries.CountAsync();
    }

    [TestMethod]
    public async Task MigrateAsync_CreatesInboxMessagesTable()
    {
        await using var serviceProvider =
            NotificationIntegrationTestHost.CreateServiceProvider();

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<NotificationDbContext>();

        await dbContext.Database.MigrateAsync();

        var connection = dbContext.Database.GetDbConnection();

        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText =
            """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name = 'InboxMessages'
            );
            """;

        var result = await command.ExecuteScalarAsync();

        var tableExists = Assert.IsInstanceOfType<bool>(result);

        Assert.IsTrue(tableExists);
    }
}
