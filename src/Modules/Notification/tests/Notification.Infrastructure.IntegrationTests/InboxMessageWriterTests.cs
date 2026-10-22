using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Inbox;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class InboxMessageWriterTests
{
    [TestMethod]
    public async Task WriteAsync_WhenMessageIsDeliveredTwice_ReturnsDuplicateAndPersistsOneRow()
    {
        await using var serviceProvider =
            NotificationIntegrationTestHost.CreateServiceProvider();

        await MigrateDatabaseAsync(serviceProvider);

        var messageId = Guid.NewGuid();

        InboxMessageWriteResult firstResult;

        await using (var firstScope = serviceProvider.CreateAsyncScope())
        {
            var writer = firstScope.ServiceProvider
                .GetRequiredService<IInboxMessageWriter>();

            firstResult = await writer.WriteAsync(
                CreateInboxMessage(messageId));
        }

        InboxMessageWriteResult secondResult;

        await using (var secondScope = serviceProvider.CreateAsyncScope())
        {
            var writer = secondScope.ServiceProvider
                .GetRequiredService<IInboxMessageWriter>();

            secondResult = await writer.WriteAsync(
                CreateInboxMessage(messageId));
        }

        await using var verificationScope =
            serviceProvider.CreateAsyncScope();

        var dbContext = verificationScope.ServiceProvider
            .GetRequiredService<NotificationDbContext>();

        var persistedMessageCount = await dbContext.InboxMessages
            .AsNoTracking()
            .CountAsync(x => x.Id == messageId);

        Assert.AreEqual(
            InboxMessageWriteResult.Inserted,
            firstResult);

        Assert.AreEqual(
            InboxMessageWriteResult.Duplicate,
            secondResult);

        Assert.AreEqual(1, persistedMessageCount);
    }

    private static async Task MigrateDatabaseAsync(
        ServiceProvider serviceProvider)
    {
        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<NotificationDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    private static InboxMessage CreateInboxMessage(Guid messageId)
    {
        var receivedOnUtc = new DateTime(
            2026,
            10,
            19,
            12,
            0,
            0,
            DateTimeKind.Utc);

        return new InboxMessage(
            messageId,
            "order.order-cancelled.v1",
            "order.order-cancelled.v1",
            """{"orderId":"7d3cbce7-40f0-4e65-a18f-894da6752435"}""",
            receivedOnUtc.AddMinutes(-1),
            receivedOnUtc);
    }
}
