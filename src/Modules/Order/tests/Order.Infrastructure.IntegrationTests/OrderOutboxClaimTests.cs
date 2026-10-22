using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.IntegrationTests;

[TestClass]
public sealed class OrderOutboxClaimTests
{
    [TestMethod]
    public async Task ClaimAsync_WhenPendingMessageIsReady_ClaimsAndPersistsOwnership()
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

        var nowUtc = new DateTime(
            2026,
            9,
            28,
            12,
            0,
            0,
            DateTimeKind.Utc);

        var outboxMessage = CreateOutboxMessage(
            nowUtc.AddMinutes(-1));

        dbContext.OutboxMessages.Add(outboxMessage);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var lockId = Guid.NewGuid();
        var lockedUntilUtc = nowUtc.AddMinutes(2);

        var claimedMessages = await outboxStore.ClaimAsync(
            lockId,
            nowUtc,
            lockedUntilUtc,
            batchSize: 10);

        Assert.HasCount(1, claimedMessages);

        var claimedMessage = claimedMessages.Single();

        Assert.AreEqual(outboxMessage.Id, claimedMessage.Id);
        Assert.AreEqual(outboxMessage.StoreId, claimedMessage.StoreId);
        Assert.AreEqual(outboxMessage.EventType, claimedMessage.EventType);
        Assert.AreEqual(outboxMessage.RoutingKey, claimedMessage.RoutingKey);
        Assert.AreEqual(outboxMessage.Payload, claimedMessage.Payload);
        Assert.AreEqual(
            outboxMessage.OccurredOnUtc,
            claimedMessage.OccurredOnUtc);
        Assert.AreEqual(1, claimedMessage.AttemptCount);
        Assert.AreEqual(lockId, claimedMessage.LockId);

        var persistedMessage = await dbContext.OutboxMessages
            .AsNoTracking()
            .SingleAsync(x => x.Id == outboxMessage.Id);

        Assert.AreEqual(
            OutboxMessageStatus.Processing,
            persistedMessage.Status);

        Assert.AreEqual(1, persistedMessage.AttemptCount);
        Assert.AreEqual(lockId, persistedMessage.LockId);
        Assert.AreEqual(
            lockedUntilUtc,
            persistedMessage.LockedUntilUtc);
    }

    [TestMethod]
    public async Task ClaimAsync_WhenClaimExpires_AllowsMessageToBeReclaimed()
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

        var nowUtc = new DateTime(
            2026,
            9,
            28,
            12,
            0,
            0,
            DateTimeKind.Utc);

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

        var secondLockId = Guid.NewGuid();

        var claimBeforeExpiration =
            await outboxStore.ClaimAsync(
                secondLockId,
                nowUtc.AddMinutes(1),
                nowUtc.AddMinutes(3),
                batchSize: 10);

        Assert.HasCount(0, claimBeforeExpiration);

        var claimAfterExpiration =
            await outboxStore.ClaimAsync(
                secondLockId,
                nowUtc.AddMinutes(3),
                nowUtc.AddMinutes(5),
                batchSize: 10);

        Assert.HasCount(1, claimAfterExpiration);

        var reclaimedMessage =
            claimAfterExpiration.Single();

        Assert.AreEqual(
            outboxMessage.Id,
            reclaimedMessage.Id);

        Assert.AreEqual(
            secondLockId,
            reclaimedMessage.LockId);

        Assert.AreEqual(
            2,
            reclaimedMessage.AttemptCount);

        var persistedMessage = await dbContext.OutboxMessages
            .AsNoTracking()
            .SingleAsync(x => x.Id == outboxMessage.Id);

        Assert.AreEqual(
            OutboxMessageStatus.Processing,
            persistedMessage.Status);

        Assert.AreEqual(2, persistedMessage.AttemptCount);
        Assert.AreEqual(secondLockId, persistedMessage.LockId);
        Assert.AreEqual(
            nowUtc.AddMinutes(5),
            persistedMessage.LockedUntilUtc);
    }

    [TestMethod]
    public async Task ClaimAsync_WhenTwoWorkersClaimConcurrently_DoesNotClaimSameMessageTwice()
    {
        await using var serviceProvider =
            OrderIntegrationTestHost.CreateServiceProvider();

        await using var setupScope =
            serviceProvider.CreateAsyncScope();

        var setupDbContext = setupScope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

        await PrepareDatabaseAsync(setupDbContext);

        var nowUtc = new DateTime(
            2026,
            9,
            29,
            12,
            0,
            0,
            DateTimeKind.Utc);

        var outboxMessages = Enumerable
            .Range(1, 10)
            .Select(index =>
                CreateOutboxMessage(
                    nowUtc.AddMinutes(-10).AddSeconds(index)))
            .ToArray();

        setupDbContext.OutboxMessages.AddRange(
            outboxMessages);

        await setupDbContext.SaveChangesAsync();

        setupDbContext.ChangeTracker.Clear();

        await using var firstWorkerScope =
            serviceProvider.CreateAsyncScope();

        await using var secondWorkerScope =
            serviceProvider.CreateAsyncScope();

        var firstOutboxStore = firstWorkerScope.ServiceProvider
            .GetRequiredService<IOrderOutboxStore>();

        var secondOutboxStore = secondWorkerScope.ServiceProvider
            .GetRequiredService<IOrderOutboxStore>();

        var firstLockId = Guid.NewGuid();
        var secondLockId = Guid.NewGuid();

        var firstClaimTask = firstOutboxStore.ClaimAsync(
            firstLockId,
            nowUtc,
            nowUtc.AddMinutes(2),
            batchSize: 5);

        var secondClaimTask = secondOutboxStore.ClaimAsync(
            secondLockId,
            nowUtc,
            nowUtc.AddMinutes(2),
            batchSize: 5);

        await Task.WhenAll(
            firstClaimTask,
            secondClaimTask);

        var firstClaim = await firstClaimTask;
        var secondClaim = await secondClaimTask;

        Assert.HasCount(5, firstClaim);
        Assert.HasCount(5, secondClaim);

        Assert.IsTrue(
            firstClaim.All(message =>
                message.LockId == firstLockId));

        Assert.IsTrue(
            secondClaim.All(message =>
                message.LockId == secondLockId));

        var allClaimedMessages = firstClaim
            .Concat(secondClaim)
            .ToArray();

        Assert.HasCount(10, allClaimedMessages);

        var distinctClaimedIds = allClaimedMessages
            .Select(message => message.Id)
            .Distinct()
            .ToArray();

        Assert.HasCount(10, distinctClaimedIds);

        Assert.IsTrue(
            outboxMessages.All(outboxMessage =>
                distinctClaimedIds.Contains(
                    outboxMessage.Id)));

        var persistedMessages =
            await setupDbContext.OutboxMessages
                .AsNoTracking()
                .ToListAsync();

        Assert.HasCount(10, persistedMessages);

        Assert.IsTrue(
            persistedMessages.All(message =>
                message.Status ==
                OutboxMessageStatus.Processing));

        Assert.IsTrue(
            persistedMessages.All(message =>
                message.AttemptCount == 1));

        Assert.AreEqual(
            5,
            persistedMessages.Count(message =>
                message.LockId == firstLockId));

        Assert.AreEqual(
            5,
            persistedMessages.Count(message =>
                message.LockId == secondLockId));
    }

    private static async Task PrepareDatabaseAsync(
        OrderDbContext dbContext)
    {
        await dbContext.Database.MigrateAsync();

        await dbContext.OutboxMessages.ExecuteDeleteAsync();

        dbContext.ChangeTracker.Clear();
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