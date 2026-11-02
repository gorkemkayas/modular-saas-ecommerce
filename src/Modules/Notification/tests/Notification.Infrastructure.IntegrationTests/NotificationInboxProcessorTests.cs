using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Infrastructure.Messaging.Inbox;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Inbox;
using Order.Contracts.IntegrationEvents;
using Notification.Infrastructure.Persistence.Dispatch;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class NotificationInboxProcessorTests
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private static readonly DateTime NowUtc =
        new(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);

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
        await dbContext.NotificationDispatches.ExecuteDeleteAsync();
    }

    [TestMethod]
    public async Task ProcessBatchAsync_WhenMessageIsValid_CreatesDispatchAndMarksInboxProcessed()
    {
        NotificationInboxOptions options = CreateOptions();

        await using ServiceProvider serviceProvider =
            CreateServiceProvider(options);

        Guid messageId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();

        await SeedOrderCancelledMessageAsync(
            serviceProvider,
            messageId,
            orderId,
            receivedOnUtc: NowUtc.AddMinutes(-1));

        NotificationInboxProcessor processor =
            serviceProvider.GetRequiredService<
                NotificationInboxProcessor>();

        await processor.ProcessBatchAsync();

        await using AsyncServiceScope scope =
            serviceProvider.CreateAsyncScope();

        NotificationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<
                NotificationDbContext>();

        InboxMessage inboxMessage =
            await dbContext.InboxMessages
                .AsNoTracking()
                .SingleAsync(message => message.Id == messageId);

        NotificationDispatch dispatch =
            await dbContext.NotificationDispatches
                .AsNoTracking()
                .SingleAsync();

        Assert.AreEqual(
            InboxMessageStatus.Processed,
            inboxMessage.Status);
        Assert.AreEqual(1, inboxMessage.AttemptCount);
        Assert.AreEqual(NowUtc, inboxMessage.ProcessedOnUtc);
        Assert.IsNull(inboxMessage.LockId);
        Assert.IsNull(inboxMessage.LockedUntilUtc);
        Assert.IsNull(inboxMessage.LastError);
        Assert.AreEqual(orderId, dispatch.BusinessEntityId);
        Assert.AreEqual(
            NotificationStatus.Suppressed,
            dispatch.Status);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_WhenTransientFailureOccurs_RollsBackDispatchAndSchedulesRetry()
    {
        NotificationInboxOptions options = CreateOptions();

        await using ServiceProvider serviceProvider =
            CreateServiceProvider(
                options,
                services =>
                    services.AddScoped<
                        INotificationInboxMessageHandler,
                        PersistThenThrowInboxMessageHandler>());

        Guid messageId = Guid.NewGuid();

        await SeedInboxMessageAsync(
            serviceProvider,
            messageId,
            PersistThenThrowInboxMessageHandler.SupportedEventType,
            "{}",
            NowUtc.AddMinutes(-1));

        NotificationInboxProcessor processor =
            serviceProvider.GetRequiredService<
                NotificationInboxProcessor>();

        await processor.ProcessBatchAsync();

        InboxMessage inboxMessage =
            await GetInboxMessageAsync(
                serviceProvider,
                messageId);

        await using AsyncServiceScope scope =
            serviceProvider.CreateAsyncScope();

        NotificationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<
                NotificationDbContext>();

        int dispatchCount =
            await dbContext.NotificationDispatches.CountAsync();

        Assert.AreEqual(0, dispatchCount);
        Assert.AreEqual(0, await dbContext.NotificationDispatchDeliveries.CountAsync());
        Assert.AreEqual(
            InboxMessageStatus.Pending,
            inboxMessage.Status);
        Assert.AreEqual(1, inboxMessage.AttemptCount);
        Assert.AreEqual(
            NowUtc.Add(options.InitialRetryDelay),
            inboxMessage.NextAttemptAtUtc);
        Assert.IsNull(inboxMessage.LockId);
        Assert.IsNull(inboxMessage.LockedUntilUtc);
        StringAssert.Contains(
            inboxMessage.LastError,
            nameof(TransientTestException));
    }

    [TestMethod]
    public async Task ProcessBatchAsync_WhenMaximumAttemptsIsReached_MarksInboxFailed()
    {
        NotificationInboxOptions options = CreateOptions();
        options.MaximumAttempts = 1;

        await using ServiceProvider serviceProvider =
            CreateServiceProvider(
                options,
                services =>
                    services.AddScoped<
                        INotificationInboxMessageHandler,
                        PersistThenThrowInboxMessageHandler>());

        Guid messageId = Guid.NewGuid();

        await SeedInboxMessageAsync(
            serviceProvider,
            messageId,
            PersistThenThrowInboxMessageHandler.SupportedEventType,
            "{}",
            NowUtc.AddMinutes(-1));

        NotificationInboxProcessor processor =
            serviceProvider.GetRequiredService<
                NotificationInboxProcessor>();

        await processor.ProcessBatchAsync();

        InboxMessage inboxMessage =
            await GetInboxMessageAsync(
                serviceProvider,
                messageId);

        Assert.AreEqual(
            InboxMessageStatus.Failed,
            inboxMessage.Status);
        Assert.AreEqual(1, inboxMessage.AttemptCount);
        Assert.IsNull(inboxMessage.LockId);
        Assert.IsNull(inboxMessage.LockedUntilUtc);
        StringAssert.Contains(
            inboxMessage.LastError,
            nameof(TransientTestException));
    }

    [TestMethod]
    public async Task ProcessBatchAsync_WhenEventTypeIsUnsupported_MarksInboxFailedWithoutRetry()
    {
        await using ServiceProvider serviceProvider =
            CreateServiceProvider(CreateOptions());

        Guid messageId = Guid.NewGuid();

        await SeedInboxMessageAsync(
            serviceProvider,
            messageId,
            "unsupported.event.v1",
            "{}",
            NowUtc.AddMinutes(-1));

        NotificationInboxProcessor processor =
            serviceProvider.GetRequiredService<
                NotificationInboxProcessor>();

        await processor.ProcessBatchAsync();

        InboxMessage inboxMessage =
            await GetInboxMessageAsync(
                serviceProvider,
                messageId);

        Assert.AreEqual(
            InboxMessageStatus.Failed,
            inboxMessage.Status);
        Assert.AreEqual(1, inboxMessage.AttemptCount);
        StringAssert.Contains(
            inboxMessage.LastError,
            nameof(UnsupportedNotificationInboxEventException));
    }

    [TestMethod]
    public async Task ProcessBatchAsync_WhenPayloadDoesNotMatchMessage_MarksInboxFailedWithoutRetry()
    {
        await using ServiceProvider serviceProvider =
            CreateServiceProvider(CreateOptions());

        Guid messageId = Guid.NewGuid();

        await SeedInboxMessageAsync(
            serviceProvider,
            messageId,
            OrderCancelledIntegrationEvent.EventType,
            "{}",
            NowUtc.AddMinutes(-1));

        NotificationInboxProcessor processor =
            serviceProvider.GetRequiredService<
                NotificationInboxProcessor>();

        await processor.ProcessBatchAsync();

        InboxMessage inboxMessage =
            await GetInboxMessageAsync(
                serviceProvider,
                messageId);

        Assert.AreEqual(
            InboxMessageStatus.Failed,
            inboxMessage.Status);
        Assert.AreEqual(1, inboxMessage.AttemptCount);
        StringAssert.Contains(
            inboxMessage.LastError,
            nameof(InvalidDataException));
    }

    [TestMethod]
    public async Task ProcessBatchAsync_WhenBusinessEventIsDeliveredTwice_CreatesSingleDispatch()
    {
        NotificationInboxOptions options = CreateOptions();

        await using ServiceProvider serviceProvider =
            CreateServiceProvider(options);

        Guid firstMessageId = Guid.NewGuid();
        Guid secondMessageId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();
        Guid storeId = Guid.NewGuid();

        await SeedOrderCancelledMessageAsync(
            serviceProvider,
            firstMessageId,
            orderId,
            NowUtc.AddMinutes(-2),
            storeId);

        await SeedOrderCancelledMessageAsync(
            serviceProvider,
            secondMessageId,
            orderId,
            NowUtc.AddMinutes(-1),
            storeId);

        NotificationInboxProcessor processor =
            serviceProvider.GetRequiredService<
                NotificationInboxProcessor>();

        await processor.ProcessBatchAsync();

        await using AsyncServiceScope scope =
            serviceProvider.CreateAsyncScope();

        NotificationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<
                NotificationDbContext>();

        int processedMessageCount =
            await dbContext.InboxMessages.CountAsync(
                message =>
                    message.Status ==
                    InboxMessageStatus.Processed);

        int dispatchCount =
            await dbContext.NotificationDispatches.CountAsync();

        Assert.AreEqual(2, processedMessageCount);
        Assert.AreEqual(1, dispatchCount);
    }

    private static ServiceProvider CreateServiceProvider(
        NotificationInboxOptions options,
        Action<IServiceCollection>? configureServices = null)
    {
        return NotificationIntegrationTestHost.CreateServiceProvider(
            configureServices,
            options,
            new FixedTimeProvider(NowUtc));
    }

    private static NotificationInboxOptions CreateOptions()
    {
        return new NotificationInboxOptions
        {
            BatchSize = 20,
            ClaimDuration = TimeSpan.FromMinutes(2),
            MaximumAttempts = 3,
            InitialRetryDelay = TimeSpan.FromSeconds(5),
            MaximumRetryDelay = TimeSpan.FromMinutes(5),
            RetryJitterFactor = 0
        };
    }

    private static async Task SeedOrderCancelledMessageAsync(
        ServiceProvider serviceProvider,
        Guid messageId,
        Guid orderId,
        DateTime receivedOnUtc,
        Guid? storeId = null)
    {
        var integrationEvent =
            new OrderCancelledIntegrationEvent(
                messageId,
                receivedOnUtc.AddMinutes(-1),
                orderId,
                storeId ?? Guid.NewGuid(),
                Guid.NewGuid(),
                $"ORD-{orderId:N}",
                " ",
                "Test Customer",
                "Test cancellation");

        await SeedInboxMessageAsync(
            serviceProvider,
            messageId,
            OrderCancelledIntegrationEvent.EventType,
            JsonSerializer.Serialize(
                integrationEvent,
                JsonOptions),
            receivedOnUtc);
    }

    private static async Task SeedInboxMessageAsync(
        ServiceProvider serviceProvider,
        Guid messageId,
        string eventType,
        string payload,
        DateTime receivedOnUtc)
    {
        await using AsyncServiceScope scope =
            serviceProvider.CreateAsyncScope();

        NotificationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<
                NotificationDbContext>();

        dbContext.InboxMessages.Add(
            new InboxMessage(
                messageId,
                eventType,
                eventType,
                payload,
                receivedOnUtc.AddMinutes(-1),
                receivedOnUtc));

        await dbContext.SaveChangesAsync();
    }

    private static async Task<InboxMessage> GetInboxMessageAsync(
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

    private sealed class PersistThenThrowInboxMessageHandler
        : INotificationInboxMessageHandler
    {
        public const string SupportedEventType =
            "test.persist-then-throw.v1";

        private readonly NotificationDbContext _dbContext;

        public PersistThenThrowInboxMessageHandler(
            NotificationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string EventType => SupportedEventType;

        public async Task HandleAsync(
            ClaimedInboxMessage message,
            CancellationToken cancellationToken = default)
        {
            NotificationDispatch dispatch =
                NotificationDispatch.Create(
                    Guid.NewGuid(),
                    NotificationChannel.Email,
                    NotificationTrigger.OrderCancelled,
                    "Order",
                    message.Id,
                    customerId: null,
                    recipientAddress: null,
                    recipientName: null);

            _dbContext.NotificationDispatches.Add(dispatch);
            _dbContext.NotificationDispatchDeliveries.Add(
                new NotificationDispatchDelivery(dispatch.Id, DateTime.UtcNow));

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            throw new TransientTestException();
        }
    }

    private sealed class TransientTestException : Exception;

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTime utcNow)
        {
            _utcNow = new DateTimeOffset(utcNow);
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
