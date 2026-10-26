using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Dispatch;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class NotificationDispatchStoreTests
{
    [TestInitialize]
    public Task InitializeAsync() => NotificationDispatchTestData.ResetAsync();

    [TestMethod]
    public async Task ClaimAsync_ConcurrentWorkersClaimDisjointBatches()
    {
        var clock = new DispatchTestTimeProvider();
        await using var provider = NotificationIntegrationTestHost.CreateServiceProvider();
        for (int i = 0; i < 30; i++)
            await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);

        async Task<IReadOnlyList<ClaimedNotificationDispatch>> ClaimAsync()
        {
            await using var scope = provider.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<INotificationDispatchStore>()
                .ClaimAsync(Guid.NewGuid(), clock.UtcNow, clock.UtcNow.AddMinutes(2), 10);
        }

        var batches = await Task.WhenAll(ClaimAsync(), ClaimAsync(), ClaimAsync());
        var claims = batches.SelectMany(x => x).ToArray();

        Assert.HasCount(30, claims);
        Assert.AreEqual(30, claims.Select(x => x.Id).Distinct().Count());
        Assert.IsTrue(claims.All(x => x.AttemptCount == 1));
    }

    [TestMethod]
    public async Task ClaimAsync_SkipsRowsLockedByAnotherTransaction()
    {
        var clock = new DispatchTestTimeProvider();
        await using var provider = NotificationIntegrationTestHost.CreateServiceProvider();
        Guid lockedId = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);
        Guid availableId = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);
        await using var lockedScope = provider.CreateAsyncScope();
        var db = lockedScope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM \"NotificationDispatchDeliveries\" WHERE \"Id\" = {lockedId} FOR UPDATE");

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await using var claimScope = provider.CreateAsyncScope();
        var claims = await claimScope.ServiceProvider.GetRequiredService<INotificationDispatchStore>()
            .ClaimAsync(Guid.NewGuid(), clock.UtcNow, clock.UtcNow.AddMinutes(2), 10, timeout.Token);

        Assert.AreEqual(availableId, claims.Single().Id);
        await transaction.RollbackAsync();
    }

    [TestMethod]
    public async Task ExpiredClaim_CannotBeRenewedAndPreviousOwnerCannotFinalize()
    {
        var clock = new DispatchTestTimeProvider();
        await using var provider = NotificationIntegrationTestHost.CreateServiceProvider();
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);
        await using var scope = provider.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<INotificationDispatchStore>();
        Guid firstLock = Guid.NewGuid();
        await store.ClaimAsync(firstLock, clock.UtcNow, clock.UtcNow.AddMinutes(2), 10);
        clock.Advance(TimeSpan.FromMinutes(3));

        Assert.IsFalse(await store.RenewLeaseAsync(id, firstLock, clock.UtcNow, clock.UtcNow.AddMinutes(2)));
        Guid secondLock = Guid.NewGuid();
        var reclaimed = await store.ClaimAsync(secondLock, clock.UtcNow, clock.UtcNow.AddMinutes(2), 10);
        Assert.AreEqual(2, reclaimed.Single().AttemptCount);
        Assert.IsFalse(await store.MarkCompletedAsync(id, firstLock));
        Assert.IsFalse(await store.MarkFailedAsync(id, firstLock, "stale"));
        Assert.IsFalse(await store.ScheduleRetryAsync(id, firstLock, clock.UtcNow, "stale"));
        Assert.IsFalse(await store.PrepareAsync(id, firstLock, "{}", "Test", clock.UtcNow));
        Assert.IsTrue(await store.MarkCompletedAsync(id, secondLock));
    }

    [TestMethod]
    public async Task PrepareAsync_PreservesOriginalPayloadAndProviderAcrossClaims()
    {
        var clock = new DispatchTestTimeProvider();
        await using var provider = NotificationIntegrationTestHost.CreateServiceProvider();
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);
        await using var scope = provider.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<INotificationDispatchStore>();
        Guid firstLock = Guid.NewGuid();
        await store.ClaimAsync(firstLock, clock.UtcNow, clock.UtcNow.AddMinutes(2), 10);
        Assert.IsTrue(await store.PrepareAsync(id, firstLock, "original", "ProviderA", clock.UtcNow));
        await store.ScheduleRetryAsync(id, firstLock, clock.UtcNow.AddSeconds(5), "retry");
        Assert.IsEmpty(await store.ClaimAsync(Guid.NewGuid(), clock.UtcNow, clock.UtcNow.AddMinutes(2), 10));
        clock.Advance(TimeSpan.FromSeconds(5));
        Guid secondLock = Guid.NewGuid();
        var claimed = (await store.ClaimAsync(secondLock, clock.UtcNow, clock.UtcNow.AddMinutes(2), 10)).Single();
        Assert.AreEqual("original", claimed.Payload);
        Assert.AreEqual("ProviderA", claimed.ProviderName);
        Assert.IsTrue(await store.PrepareAsync(id, secondLock, "changed", "ProviderB", clock.UtcNow));

        var (_, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.AreEqual("original", delivery.Payload);
        Assert.AreEqual("ProviderA", delivery.ProviderName);
        Assert.AreEqual(clock.UtcNow.AddSeconds(-5), delivery.PreparedAtUtc);
    }
}
