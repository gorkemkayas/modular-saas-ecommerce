using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Abstractions;
using Notification.Infrastructure.Messaging.Inbox;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Inbox;
using Payment.Contracts.IntegrationEvents;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class RecipientSnapshotTests
{
    [TestMethod]
    public async Task CompleteSnapshot_DoesNotReadOrder()
    {
        var reader = new Reader { Fail = true };
        var recipient = await new NotificationRecipientResolver(reader).ResolveAsync(Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "snapshot@example.com", "Snapshot Customer");
        Assert.AreEqual("snapshot@example.com", recipient.Email);
        Assert.AreEqual(0, reader.Calls);
    }

    [TestMethod]
    public async Task LegacySnapshot_UsesPublicReader_AndRejectsDifferentCustomer()
    {
        var reader = new Reader();
        var resolver = new NotificationRecipientResolver(reader);
        Assert.AreEqual(reader.Result, await resolver.ResolveAsync(Guid.NewGuid(), Guid.NewGuid(), null, null, null));
        await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
            resolver.ResolveAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null));
    }

    [TestMethod]
    public async Task LegacyOrderReadFailure_OnlySchedulesInboxRetry_ThenRecovers()
    {
        await NotificationDispatchTestData.ResetAsync();
        var reader = new Reader { Fail = true };
        await using var provider = NotificationIntegrationTestHost.CreateServiceProvider(
            services => services.AddSingleton<IOrderRecipientReader>(reader));
        var e = new PaymentStatusChangedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), reader.Result.CustomerId, "ORD-LEGACY", PaymentEventKind.Captured,
            100m, "TRY", "reference", null, null, null, null);
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            db.InboxMessages.Add(new InboxMessage(e.Id, PaymentStatusChangedIntegrationEvent.EventType,
                PaymentStatusChangedIntegrationEvent.EventType,
                JsonSerializer.Serialize(e, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
                e.OccurredOnUtc, DateTime.UtcNow.AddMinutes(-1)));
            await db.SaveChangesAsync();
        }

        await provider.GetRequiredService<NotificationInboxProcessor>().ProcessBatchAsync();
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            var row = await db.InboxMessages.AsNoTracking().SingleAsync();
            Assert.AreEqual(InboxMessageStatus.Pending, row.Status);
            Assert.IsNotNull(row.LastError);
            Assert.AreEqual(0, await db.NotificationDispatches.CountAsync());
            await db.InboxMessages.ExecuteUpdateAsync(update => update.SetProperty(x => x.NextAttemptAtUtc, DateTime.UtcNow.AddMinutes(-1)));
        }
        reader.Fail = false;
        await provider.GetRequiredService<NotificationInboxProcessor>().ProcessBatchAsync();
        await using var verification = provider.CreateAsyncScope();
        var verify = verification.ServiceProvider.GetRequiredService<NotificationDbContext>();
        Assert.AreEqual(InboxMessageStatus.Processed, (await verify.InboxMessages.SingleAsync()).Status);
        Assert.AreEqual(1, await verify.NotificationDispatches.CountAsync());
    }

    private sealed class Reader : IOrderRecipientReader
    {
        public int Calls { get; private set; }
        public bool Fail { get; set; }
        public OrderNotificationRecipient Result { get; } = new(Guid.NewGuid(), "legacy@example.com", "Legacy Customer");
        public Task<OrderNotificationRecipient?> GetAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default)
        {
            Calls++;
            if (Fail) throw new IOException("Order unavailable");
            return Task.FromResult<OrderNotificationRecipient?>(Result);
        }
    }
}
