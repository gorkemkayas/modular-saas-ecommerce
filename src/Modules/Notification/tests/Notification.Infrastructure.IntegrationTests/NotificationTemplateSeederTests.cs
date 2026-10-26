using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Seeding;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class NotificationTemplateSeederTests
{
    [TestMethod]
    public async Task SeedAsync_RepeatedCalls_CreateOneDefaultPerTrigger()
    {
        await using var provider = NotificationIntegrationTestHost.CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await db.Database.MigrateAsync();
        var seeder = new NotificationTemplateSeeder(
            db, NullLogger<NotificationTemplateSeeder>.Instance);

        await seeder.SeedAsync();
        var initialCount = await db.NotificationTemplates.CountAsync(x => x.StoreId == Guid.Empty);
        await seeder.SeedAsync();

        Assert.IsGreaterThan(0, initialCount);
        Assert.AreEqual(initialCount,
            await db.NotificationTemplates.CountAsync(x => x.StoreId == Guid.Empty));
        Assert.AreEqual(1, await db.NotificationTemplates.CountAsync(x =>
            x.StoreId == Guid.Empty && x.Trigger == NotificationTrigger.OrderCancelled &&
            x.Channel == NotificationChannel.Email && x.Locale == "default" && x.IsActive));
    }
}
