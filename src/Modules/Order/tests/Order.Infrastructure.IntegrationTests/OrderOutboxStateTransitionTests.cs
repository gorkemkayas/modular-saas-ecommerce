using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.IntegrationTests;

[TestClass]
public sealed class OrderOutboxStateTransitionTests
{
    [TestMethod]
    public async Task MarkPublishedAsync_WithCorrectLockId_MarksMessageAsPublished()
    {
        await using var serviceProvider =
            OrderIntegrationTestHost.CreateServiceProvider();

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

        var outboxStore = scope.ServiceProvider
            .GetRequiredService<IOrderOutboxStore>();

        await PrepareDatabaseAsync(dbContext);

        var nowUtc = CreateCurrentTime();
        var outboxMessage = CreateOutboxMessage(
            nowUtc.AddMinutes(-1));

        dbContext.OutboxMessages.Add(outboxMessage);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var lockId = Guid.NewGuid();

        var claimedMessages = await outboxStore.ClaimAsync(
            lockId,
            nowUtc,
            nowUtc.AddMinutes(2),
            batchSize: 10);

        Assert.HasCount(1, claimedMessages);

        var resultWithWrongLock =
            await outboxStore.MarkPublishedAsync(
                outboxMessage.Id,
                Guid.NewGuid(),
                nowUtc);

        Assert.IsFalse(resultWithWrongLock);

        var messageAfterWrongLock =
            await dbContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x => x.Id == outboxMessage.Id);

        Assert.AreEqual(
            OutboxMessageStatus.Processing,
            messageAfterWrongLock.Status);

        Assert.AreEqual(
            lockId,
            messageAfterWrongLock.LockId);

        var publishedAtUtc = nowUtc.AddSeconds(30);

        var result = await outboxStore.MarkPublishedAsync(
            outboxMessage.Id,
            lockId,
            publishedAtUtc);

        Assert.IsTrue(result);

        var publishedMessage =
            await dbContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x => x.Id == outboxMessage.Id);

        Assert.AreEqual(
            OutboxMessageStatus.Published,
            publishedMessage.Status);

        Assert.AreEqual(
            publishedAtUtc,
            publishedMessage.PublishedAtUtc);

        Assert.IsNull(publishedMessage.LockId);
        Assert.IsNull(publishedMessage.LockedUntilUtc);
        Assert.IsNull(publishedMessage.LastError);
    }

    [TestMethod]
    public async Task ScheduleRetryAsync_SchedulesMessageForFutureAttempt()
    {
        await using var serviceProvider =
            OrderIntegrationTestHost.CreateServiceProvider();

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

        var outboxStore = scope.ServiceProvider
            .GetRequiredService<IOrderOutboxStore>();

        await PrepareDatabaseAsync(dbContext);

        var nowUtc = CreateCurrentTime();
        var outboxMessage = CreateOutboxMessage(
            nowUtc.AddMinutes(-1));

        dbContext.OutboxMessages.Add(outboxMessage);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var firstLockId = Guid.NewGuid();

        var firstClaim = await outboxStore.ClaimAsync(
            firstLockId,
            nowUtc,
            nowUtc.AddMinutes(2),
            batchSize: 10);

        Assert.HasCount(1, firstClaim);

        var nextAttemptAtUtc = nowUtc.AddMinutes(5);
        const string error = "RabbitMQ is temporarily unavailable.";

        var retryScheduled =
            await outboxStore.ScheduleRetryAsync(
                outboxMessage.Id,
                firstLockId,
                nextAttemptAtUtc,
                error);

        Assert.IsTrue(retryScheduled);

        var pendingMessage =
            await dbContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x => x.Id == outboxMessage.Id);

        Assert.AreEqual(
            OutboxMessageStatus.Pending,
            pendingMessage.Status);

        Assert.AreEqual(
            nextAttemptAtUtc,
            pendingMessage.NextAttemptAtUtc);

        Assert.AreEqual(error, pendingMessage.LastError);
        Assert.IsNull(pendingMessage.LockId);
        Assert.IsNull(pendingMessage.LockedUntilUtc);
        Assert.AreEqual(1, pendingMessage.AttemptCount);

        var secondLockId = Guid.NewGuid();

        var claimBeforeRetryTime =
            await outboxStore.ClaimAsync(
                secondLockId,
                nextAttemptAtUtc.AddSeconds(-1),
                nextAttemptAtUtc.AddMinutes(2),
                batchSize: 10);

        Assert.HasCount(0, claimBeforeRetryTime);

        var claimAtRetryTime =
            await outboxStore.ClaimAsync(
                secondLockId,
                nextAttemptAtUtc,
                nextAttemptAtUtc.AddMinutes(2),
                batchSize: 10);

        Assert.HasCount(1, claimAtRetryTime);

        var retriedMessage = claimAtRetryTime.Single();

        Assert.AreEqual(outboxMessage.Id, retriedMessage.Id);
        Assert.AreEqual(secondLockId, retriedMessage.LockId);
        Assert.AreEqual(2, retriedMessage.AttemptCount);
    }

    [TestMethod]
    public async Task MarkFailedAsync_MarksMessageAsFailedAndPreventsFutureClaims()
    {
        await using var serviceProvider =
            OrderIntegrationTestHost.CreateServiceProvider();

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

        var outboxStore = scope.ServiceProvider
            .GetRequiredService<IOrderOutboxStore>();

        await PrepareDatabaseAsync(dbContext);

        var nowUtc = CreateCurrentTime();
        var outboxMessage = CreateOutboxMessage(
            nowUtc.AddMinutes(-1));

        dbContext.OutboxMessages.Add(outboxMessage);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var lockId = Guid.NewGuid();

        var claimedMessages = await outboxStore.ClaimAsync(
            lockId,
            nowUtc,
            nowUtc.AddMinutes(2),
            batchSize: 10);

        Assert.HasCount(1, claimedMessages);

        const string error =
            "Maximum publishing attempt count was reached.";

        var markedAsFailed =
            await outboxStore.MarkFailedAsync(
                outboxMessage.Id,
                lockId,
                error);

        Assert.IsTrue(markedAsFailed);

        var failedMessage =
            await dbContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x => x.Id == outboxMessage.Id);

        Assert.AreEqual(
            OutboxMessageStatus.Failed,
            failedMessage.Status);

        Assert.AreEqual(error, failedMessage.LastError);
        Assert.IsNull(failedMessage.LockId);
        Assert.IsNull(failedMessage.LockedUntilUtc);
        Assert.IsNull(failedMessage.PublishedAtUtc);

        var futureClaim = await outboxStore.ClaimAsync(
            Guid.NewGuid(),
            nowUtc.AddDays(1),
            nowUtc.AddDays(1).AddMinutes(2),
            batchSize: 10);

        Assert.HasCount(0, futureClaim);
    }

    private static async Task PrepareDatabaseAsync(
        OrderDbContext dbContext)
    {
        await dbContext.Database.MigrateAsync();

        await dbContext.OutboxMessages.ExecuteDeleteAsync();

        dbContext.ChangeTracker.Clear();
    }

    private static DateTime CreateCurrentTime()
    {
        return new DateTime(
            2026,
            9,
            28,
            12,
            0,
            0,
            DateTimeKind.Utc);
    }

    private static OutboxMessage CreateOutboxMessage(
        DateTime createdAtUtc)
    {
        return new OutboxMessage(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.test-event.v1",
            "order.test-event.v1",
            "{}",
            createdAtUtc,
            createdAtUtc);
    }
}