using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Inbox;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class NotificationInboxStoreTests
{
    [TestInitialize]
    public async Task InitializeAsync()
    {
        await using ServiceProvider serviceProvider =
            NotificationIntegrationTestHost.CreateServiceProvider();

        await using AsyncServiceScope scope =
            serviceProvider.CreateAsyncScope();

        NotificationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<
                NotificationDbContext>();

        await dbContext.Database.MigrateAsync();
        await dbContext.InboxMessages.ExecuteDeleteAsync();
    }

    [TestMethod]
    public async Task ClaimAsync_WhenTwoWorkersClaimConcurrently_DoesNotReturnOverlappingMessages()
    {
        await using ServiceProvider serviceProvider =
            NotificationIntegrationTestHost.CreateServiceProvider();

        DateTime nowUtc = DateTime.UtcNow;
        Guid[] messageIds = Enumerable
            .Range(0, 10)
            .Select(_ => Guid.NewGuid())
            .ToArray();

        await SeedMessagesAsync(
            serviceProvider,
            messageIds,
            nowUtc);

        await using AsyncServiceScope firstScope =
            serviceProvider.CreateAsyncScope();
        await using AsyncServiceScope secondScope =
            serviceProvider.CreateAsyncScope();

        INotificationInboxStore firstStore =
            firstScope.ServiceProvider.GetRequiredService<
                INotificationInboxStore>();
        INotificationInboxStore secondStore =
            secondScope.ServiceProvider.GetRequiredService<
                INotificationInboxStore>();

        Task<IReadOnlyList<ClaimedInboxMessage>> firstClaim =
            firstStore.ClaimAsync(
                Guid.NewGuid(),
                nowUtc,
                nowUtc.AddMinutes(2),
                batchSize: 5);

        Task<IReadOnlyList<ClaimedInboxMessage>> secondClaim =
            secondStore.ClaimAsync(
                Guid.NewGuid(),
                nowUtc,
                nowUtc.AddMinutes(2),
                batchSize: 5);

        await Task.WhenAll(firstClaim, secondClaim);

        Guid[] firstIds = firstClaim.Result
            .Select(message => message.Id)
            .ToArray();
        Guid[] secondIds = secondClaim.Result
            .Select(message => message.Id)
            .ToArray();

        Assert.HasCount(5, firstIds);
        Assert.HasCount(5, secondIds);
        Assert.IsEmpty(firstIds.Intersect(secondIds));
        CollectionAssert.AreEquivalent(
            messageIds,
            firstIds.Concat(secondIds).ToArray());
    }

    [TestMethod]
    public async Task ClaimAsync_WhenLeaseIsActive_DoesNotReclaimMessage()
    {
        await using ServiceProvider serviceProvider =
            NotificationIntegrationTestHost.CreateServiceProvider();

        DateTime nowUtc = DateTime.UtcNow;
        Guid messageId = Guid.NewGuid();

        await SeedMessagesAsync(
            serviceProvider,
            [messageId],
            nowUtc);

        IReadOnlyList<ClaimedInboxMessage> firstClaim =
            await ClaimAsync(
                serviceProvider,
                Guid.NewGuid(),
                nowUtc,
                nowUtc.AddMinutes(2));

        IReadOnlyList<ClaimedInboxMessage> secondClaim =
            await ClaimAsync(
                serviceProvider,
                Guid.NewGuid(),
                nowUtc.AddMinutes(1),
                nowUtc.AddMinutes(3));

        Assert.HasCount(1, firstClaim);
        Assert.IsEmpty(secondClaim);
    }

    [TestMethod]
    public async Task ClaimAsync_WhenLeaseExpires_ReclaimsMessageWithNewLockAndAttempt()
    {
        await using ServiceProvider serviceProvider =
            NotificationIntegrationTestHost.CreateServiceProvider();

        DateTime nowUtc = DateTime.UtcNow;
        Guid messageId = Guid.NewGuid();
        Guid firstLockId = Guid.NewGuid();
        Guid secondLockId = Guid.NewGuid();

        await SeedMessagesAsync(
            serviceProvider,
            [messageId],
            nowUtc);

        IReadOnlyList<ClaimedInboxMessage> firstClaim =
            await ClaimAsync(
                serviceProvider,
                firstLockId,
                nowUtc,
                nowUtc.AddMinutes(2));

        IReadOnlyList<ClaimedInboxMessage> secondClaim =
            await ClaimAsync(
                serviceProvider,
                secondLockId,
                nowUtc.AddMinutes(3),
                nowUtc.AddMinutes(5));

        Assert.HasCount(1, firstClaim);
        Assert.HasCount(1, secondClaim);

        ClaimedInboxMessage firstMessage = firstClaim[0];
        ClaimedInboxMessage secondMessage = secondClaim[0];

        Assert.AreEqual(messageId, firstMessage.Id);
        Assert.AreEqual(firstLockId, firstMessage.LockId);
        Assert.AreEqual(1, firstMessage.AttemptCount);

        Assert.AreEqual(messageId, secondMessage.Id);
        Assert.AreEqual(secondLockId, secondMessage.LockId);
        Assert.AreEqual(2, secondMessage.AttemptCount);
    }

    [TestMethod]
    public async Task MarkProcessedAsync_WhenLockDoesNotMatch_RejectsStaleWorker()
    {
        await using ServiceProvider serviceProvider =
            NotificationIntegrationTestHost.CreateServiceProvider();

        DateTime nowUtc = DateTime.UtcNow;
        Guid messageId = Guid.NewGuid();
        Guid ownerLockId = Guid.NewGuid();

        await SeedMessagesAsync(
            serviceProvider,
            [messageId],
            nowUtc);

        await ClaimAsync(
            serviceProvider,
            ownerLockId,
            nowUtc,
            nowUtc.AddMinutes(2));

        bool staleWorkerResult;
        bool ownerResult;

        await using (AsyncServiceScope scope =
                     serviceProvider.CreateAsyncScope())
        {
            INotificationInboxStore store =
                scope.ServiceProvider.GetRequiredService<
                    INotificationInboxStore>();

            staleWorkerResult = await store.MarkProcessedAsync(
                messageId,
                Guid.NewGuid(),
                nowUtc.AddSeconds(1));

            ownerResult = await store.MarkProcessedAsync(
                messageId,
                ownerLockId,
                nowUtc.AddSeconds(1));
        }

        InboxMessage persistedMessage =
            await GetMessageAsync(
                serviceProvider,
                messageId);

        Assert.IsFalse(staleWorkerResult);
        Assert.IsTrue(ownerResult);
        Assert.AreEqual(
            InboxMessageStatus.Processed,
            persistedMessage.Status);
        Assert.IsNull(persistedMessage.LockId);
        Assert.IsNull(persistedMessage.LockedUntilUtc);
        Assert.IsNotNull(
            persistedMessage.ProcessedOnUtc);

        TimeSpan timestampDifference =
            (nowUtc.AddSeconds(1) -
             persistedMessage.ProcessedOnUtc.Value)
            .Duration();

        Assert.IsTrue(
            timestampDifference < TimeSpan.FromMilliseconds(1),
            "Processed timestamp differs beyond PostgreSQL precision.");
    }

    [TestMethod]
    public async Task ScheduleRetryAsync_DefersMessageUntilNextAttemptTime()
    {
        await using ServiceProvider serviceProvider =
            NotificationIntegrationTestHost.CreateServiceProvider();

        DateTime nowUtc = DateTime.UtcNow;
        DateTime nextAttemptAtUtc = nowUtc.AddMinutes(5);
        Guid messageId = Guid.NewGuid();
        Guid lockId = Guid.NewGuid();

        await SeedMessagesAsync(
            serviceProvider,
            [messageId],
            nowUtc);

        await ClaimAsync(
            serviceProvider,
            lockId,
            nowUtc,
            nowUtc.AddMinutes(2));

        await using (AsyncServiceScope scope =
                     serviceProvider.CreateAsyncScope())
        {
            INotificationInboxStore store =
                scope.ServiceProvider.GetRequiredService<
                    INotificationInboxStore>();

            bool scheduled = await store.ScheduleRetryAsync(
                messageId,
                lockId,
                nextAttemptAtUtc,
                "Temporary failure");

            Assert.IsTrue(scheduled);
        }

        IReadOnlyList<ClaimedInboxMessage> earlyClaim =
            await ClaimAsync(
                serviceProvider,
                Guid.NewGuid(),
                nowUtc.AddMinutes(4),
                nowUtc.AddMinutes(6));

        IReadOnlyList<ClaimedInboxMessage> dueClaim =
            await ClaimAsync(
                serviceProvider,
                Guid.NewGuid(),
                nextAttemptAtUtc,
                nextAttemptAtUtc.AddMinutes(2));

        Assert.IsEmpty(earlyClaim);
        Assert.HasCount(1, dueClaim);
        Assert.AreEqual(messageId, dueClaim[0].Id);
        Assert.AreEqual(2, dueClaim[0].AttemptCount);
    }

    [TestMethod]
    public async Task MarkFailedAsync_PreventsMessageFromBeingClaimedAgain()
    {
        await using ServiceProvider serviceProvider =
            NotificationIntegrationTestHost.CreateServiceProvider();

        DateTime nowUtc = DateTime.UtcNow;
        Guid messageId = Guid.NewGuid();
        Guid lockId = Guid.NewGuid();

        await SeedMessagesAsync(
            serviceProvider,
            [messageId],
            nowUtc);

        await ClaimAsync(
            serviceProvider,
            lockId,
            nowUtc,
            nowUtc.AddMinutes(2));

        await using (AsyncServiceScope scope =
                     serviceProvider.CreateAsyncScope())
        {
            INotificationInboxStore store =
                scope.ServiceProvider.GetRequiredService<
                    INotificationInboxStore>();

            bool failed = await store.MarkFailedAsync(
                messageId,
                lockId,
                "Unsupported event type");

            Assert.IsTrue(failed);
        }

        IReadOnlyList<ClaimedInboxMessage> laterClaim =
            await ClaimAsync(
                serviceProvider,
                Guid.NewGuid(),
                nowUtc.AddDays(1),
                nowUtc.AddDays(1).AddMinutes(2));

        InboxMessage persistedMessage =
            await GetMessageAsync(
                serviceProvider,
                messageId);

        Assert.IsEmpty(laterClaim);
        Assert.AreEqual(
            InboxMessageStatus.Failed,
            persistedMessage.Status);
        Assert.AreEqual(
            "Unsupported event type",
            persistedMessage.LastError);
    }

    private static async Task<IReadOnlyList<ClaimedInboxMessage>>
        ClaimAsync(
            ServiceProvider serviceProvider,
            Guid lockId,
            DateTime nowUtc,
            DateTime lockedUntilUtc)
    {
        await using AsyncServiceScope scope =
            serviceProvider.CreateAsyncScope();

        INotificationInboxStore store =
            scope.ServiceProvider.GetRequiredService<
                INotificationInboxStore>();

        return await store.ClaimAsync(
            lockId,
            nowUtc,
            lockedUntilUtc,
            batchSize: 20);
    }

    private static async Task SeedMessagesAsync(
        ServiceProvider serviceProvider,
        IEnumerable<Guid> messageIds,
        DateTime receivedOnUtc)
    {
        await using AsyncServiceScope scope =
            serviceProvider.CreateAsyncScope();

        NotificationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<
                NotificationDbContext>();

        InboxMessage[] messages = messageIds
            .Select(
                messageId =>
                    new InboxMessage(
                        messageId,
                        "order.order-cancelled.v1",
                        "order.order-cancelled.v1",
                        "{}",
                        receivedOnUtc.AddMinutes(-1),
                        receivedOnUtc))
            .ToArray();

        dbContext.InboxMessages.AddRange(messages);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<InboxMessage> GetMessageAsync(
        ServiceProvider serviceProvider,
        Guid messageId)
    {
        await using AsyncServiceScope scope =
            serviceProvider.CreateAsyncScope();

        NotificationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<
                NotificationDbContext>();

        return await dbContext.InboxMessages
            .AsNoTracking()
            .SingleAsync(message => message.Id == messageId);
    }
}
