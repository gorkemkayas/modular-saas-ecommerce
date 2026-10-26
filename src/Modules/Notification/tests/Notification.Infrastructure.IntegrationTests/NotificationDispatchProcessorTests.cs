using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Notification.Application.Notifications.Services;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Infrastructure.Messaging.Dispatch;
using Notification.Infrastructure.Messaging.Inbox;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Dispatch;
using Notification.Infrastructure.Persistence.Inbox;
using Order.Contracts.IntegrationEvents;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class NotificationDispatchProcessorTests
{
    [TestInitialize]
    public Task InitializeAsync() => NotificationDispatchTestData.ResetAsync();

    [TestMethod]
    public async Task ProcessBatchAsync_SendsQueuedDispatchAndPersistsAttemptAtomically()
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(_ => Task.FromResult(Success()));
        await using var provider = CreateProvider(clock, gateway);
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);

        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();

        var (dispatch, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.AreEqual(NotificationStatus.Sent, dispatch.Status);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Completed, delivery.Status);
        Assert.HasCount(1, dispatch.Attempts);
        Assert.AreEqual("provider-id", dispatch.ProviderMessageId);
        Assert.IsNull(delivery.LockId);
        Assert.IsNotNull(delivery.Payload);
        Assert.AreEqual($"notification-dispatch/{id:D}", gateway.Requests.Single().IdempotencyKey);
        Assert.AreEqual(NotificationAttemptStatus.Sent, dispatch.Attempts.Single().Status);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_TransientFailureSchedulesRetryAndKeepsDispatchPending()
    {
        var clock = new DispatchTestTimeProvider();
        int calls = 0;
        var gateway = new RecordingGateway(_ => Task.FromResult(
            Interlocked.Increment(ref calls) == 1 ? Transient() : Success()));
        await using var provider = CreateProvider(clock, gateway);
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);
        var processor = provider.GetRequiredService<NotificationDispatchProcessor>();

        await processor.ProcessBatchAsync();
        var (dispatch, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.AreEqual(NotificationStatus.Pending, dispatch.Status);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Pending, delivery.Status);
        Assert.AreEqual(clock.UtcNow.AddSeconds(5), delivery.NextAttemptAtUtc);
        Assert.AreEqual(NotificationAttemptStatus.Failed, dispatch.Attempts.Single().Status);

        await processor.ProcessBatchAsync();
        Assert.HasCount(1, gateway.Requests);

        provider.GetRequiredService<IOptions<NotificationEmailOptions>>().Value.FromEmail = "changed@example.com";
        clock.Advance(TimeSpan.FromSeconds(5));
        await processor.ProcessBatchAsync();
        (dispatch, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.AreEqual(NotificationStatus.Sent, dispatch.Status);
        Assert.HasCount(2, dispatch.Attempts);
        Assert.AreEqual(2, delivery.AttemptCount);
        Assert.AreEqual(gateway.Requests.First(), gateway.Requests.Last());
    }

    [TestMethod]
    [DataRow(false, 10)]
    [DataRow(true, 1)]
    public async Task ProcessBatchAsync_PermanentOrExhaustedFailureStopsRetry(bool transient, int maxAttempts)
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(_ => Task.FromResult(
            new EmailSendResult(false, null, null, "rejected", "Cannot send", transient)));
        var options = CreateOptions();
        options.MaximumAttempts = maxAttempts;
        await using var provider = CreateProvider(clock, gateway, options);
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);

        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();
        var (dispatch, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);

        Assert.AreEqual(NotificationStatus.Failed, dispatch.Status);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Failed, delivery.Status);
        Assert.HasCount(1, dispatch.Attempts);
        Assert.IsNull(delivery.LockId);
        clock.Advance(TimeSpan.FromDays(1));
        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();
        Assert.HasCount(1, gateway.Requests);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_HonorsProviderRetryAfter()
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(_ => Task.FromResult(
            Transient() with { RetryAfter = TimeSpan.FromMinutes(2) }));
        await using var provider = CreateProvider(clock, gateway);
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);

        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();

        var (_, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.AreEqual(clock.UtcNow.AddMinutes(2), delivery.NextAttemptAtUtc);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_RepeatedFailuresUseCappedExponentialBackoff()
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(_ => Task.FromResult(Transient()));
        var options = CreateOptions();
        options.MaximumRetryDelay = TimeSpan.FromSeconds(8);
        await using var provider = CreateProvider(clock, gateway, options);
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);
        var processor = provider.GetRequiredService<NotificationDispatchProcessor>();

        foreach (int expectedSeconds in new[] { 5, 8, 8 })
        {
            await processor.ProcessBatchAsync();
            var (_, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
            Assert.AreEqual(clock.UtcNow.AddSeconds(expectedSeconds), delivery.NextAttemptAtUtc);
            clock.Advance(TimeSpan.FromSeconds(expectedSeconds));
        }
        Assert.HasCount(3, gateway.Requests);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_JitterStaysWithinConfiguredRange()
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(_ => Task.FromResult(Transient()));
        var options = CreateOptions();
        options.InitialRetryDelay = TimeSpan.FromSeconds(10);
        options.RetryJitterFactor = 0.2;
        await using var provider = CreateProvider(clock, gateway, options);
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);

        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();

        var (_, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.IsTrue(delivery.NextAttemptAtUtc >= clock.UtcNow.AddSeconds(8));
        Assert.IsTrue(delivery.NextAttemptAtUtc <= clock.UtcNow.AddSeconds(12));
    }

    [TestMethod]
    public async Task ProcessBatchAsync_SendTimeoutSchedulesRetry()
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(async token =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return Success();
        });
        var options = CreateOptions();
        options.SendTimeout = TimeSpan.FromMilliseconds(100);
        await using var provider = CreateProvider(clock, gateway, options);
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);

        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();

        var (dispatch, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.AreEqual(NotificationStatus.Pending, dispatch.Status);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Pending, delivery.Status);
        Assert.AreEqual("delivery_error", dispatch.FailureCode);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_ShutdownLeavesClaimRecoverableWithoutRecordingFailure()
    {
        var clock = new DispatchTestTimeProvider();
        using var cancellation = new CancellationTokenSource();
        var gateway = new RecordingGateway(_ =>
        {
            cancellation.Cancel();
            return Task.FromCanceled<EmailSendResult>(cancellation.Token);
        });
        await using var provider = CreateProvider(clock, gateway);
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync(cancellation.Token));

        var (dispatch, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.AreEqual(NotificationStatus.Pending, dispatch.Status);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Processing, delivery.Status);
        Assert.IsEmpty(dispatch.Attempts);
        Assert.IsNotNull(delivery.LockedUntilUtc);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_ReusesFrozenRequestAfterOutcomePersistenceFails()
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(_ => Task.FromResult(Success()));
        var interceptor = new FailFirstAttemptSaveInterceptor();
        await using var provider = CreateProvider(clock, gateway, configure: services =>
            services.AddDbContext<NotificationDbContext>(options => options.AddInterceptors(interceptor)));
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);
        var processor = provider.GetRequiredService<NotificationDispatchProcessor>();

        await processor.ProcessBatchAsync();
        var (dispatch, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.AreEqual(NotificationStatus.Pending, dispatch.Status);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Processing, delivery.Status);
        Assert.IsEmpty(dispatch.Attempts);
        Assert.IsNotNull(delivery.Payload);

        clock.Advance(TimeSpan.FromMinutes(3));
        await processor.ProcessBatchAsync();
        (dispatch, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.AreEqual(NotificationStatus.Sent, dispatch.Status);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Completed, delivery.Status);
        Assert.HasCount(2, gateway.Requests);
        Assert.AreEqual(gateway.Requests.First(), gateway.Requests.Last());
        Assert.HasCount(1, dispatch.Attempts);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_WhenClaimIsStolen_DiscardsStaleProviderOutcome()
    {
        var clock = new DispatchTestTimeProvider();
        ServiceProvider? provider = null;
        var gateway = new RecordingGateway(async _ =>
        {
            clock.Advance(TimeSpan.FromMinutes(3));
            await using var scope = provider!.CreateAsyncScope();
            var store = scope.ServiceProvider.GetRequiredService<INotificationDispatchStore>();
            var claims = await store.ClaimAsync(Guid.NewGuid(), clock.UtcNow, clock.UtcNow.AddMinutes(2), 20);
            Assert.HasCount(1, claims);
            return Success();
        });
        await using var services = CreateProvider(clock, gateway);
        provider = services;
        Guid id = await NotificationDispatchTestData.SeedAsync(services, clock.UtcNow);

        await services.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();

        var (dispatch, delivery) = await NotificationDispatchTestData.LoadAsync(services, id);
        Assert.AreEqual(NotificationStatus.Pending, dispatch.Status);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Processing, delivery.Status);
        Assert.AreEqual(2, delivery.AttemptCount);
        Assert.IsEmpty(dispatch.Attempts);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_ExpiredIdempotencyWindowStopsAutomaticResend()
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(_ => Task.FromResult(Transient()));
        await using var provider = CreateProvider(clock, gateway);
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);
        var processor = provider.GetRequiredService<NotificationDispatchProcessor>();
        await processor.ProcessBatchAsync();
        clock.Advance(TimeSpan.FromHours(24));

        await processor.ProcessBatchAsync();

        var (dispatch, delivery) = await NotificationDispatchTestData.LoadAsync(provider, id);
        Assert.AreEqual(NotificationStatus.Failed, dispatch.Status);
        Assert.AreEqual("idempotency_window_expired", dispatch.FailureCode);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Failed, delivery.Status);
        Assert.HasCount(1, gateway.Requests);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_DoesNotPickUpLegacySynchronousDispatches()
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(_ => Task.FromResult(Success()));
        await using var provider = CreateProvider(clock, gateway);
        Guid id = await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow, enqueue: false);

        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        Assert.AreEqual(NotificationStatus.Pending,
            (await db.NotificationDispatches.SingleAsync(x => x.Id == id)).Status);
        Assert.IsEmpty(gateway.Requests);
    }

    [TestMethod]
    public async Task ProcessBatchAsync_ConcurrentInvocationsSendEachDispatchOnce()
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(async token =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10), token);
            return Success();
        });
        var options = CreateOptions();
        options.BatchSize = 5;
        await using var provider = CreateProvider(clock, gateway, options);
        for (int i = 0; i < 10; i++)
            await NotificationDispatchTestData.SeedAsync(provider, clock.UtcNow);

        var processor = provider.GetRequiredService<NotificationDispatchProcessor>();
        await Task.WhenAll(processor.ProcessBatchAsync(), processor.ProcessBatchAsync());

        Assert.HasCount(10, gateway.Requests);
        Assert.AreEqual(10, gateway.Requests.Select(x => x.IdempotencyKey).Distinct().Count());
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        Assert.AreEqual(10, await db.NotificationDispatchDeliveries.CountAsync(
            x => x.Status == NotificationDispatchDeliveryStatus.Completed));
    }

    [TestMethod]
    public async Task InboxToDispatchToGateway_PersistsAndProcessesDeliveryJob()
    {
        var clock = new DispatchTestTimeProvider();
        var gateway = new RecordingGateway(_ => Task.FromResult(Success()));
        await using var provider = CreateProvider(clock, gateway);
        Guid storeId = Guid.NewGuid();
        Guid messageId = Guid.NewGuid();
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            db.NotificationTemplates.Add(NotificationTemplate.Create(storeId, "Cancellation",
                NotificationTrigger.OrderCancelled, NotificationChannel.Email, "default",
                "Order {{OrderNumber}} cancelled", "Hello {{RecipientName}}"));
            var integrationEvent = new OrderCancelledIntegrationEvent(messageId, clock.UtcNow,
                Guid.NewGuid(), storeId, Guid.NewGuid(), "ORD-1", "customer@example.com", "Customer", "Cancelled");
            db.InboxMessages.Add(new InboxMessage(messageId, OrderCancelledIntegrationEvent.EventType,
                OrderCancelledIntegrationEvent.EventType,
                JsonSerializer.Serialize(integrationEvent, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
                clock.UtcNow, clock.UtcNow));
            await db.SaveChangesAsync();
        }

        await provider.GetRequiredService<NotificationInboxProcessor>().ProcessBatchAsync();
        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();

        await using var finalScope = provider.CreateAsyncScope();
        var context = finalScope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        Assert.AreEqual(InboxMessageStatus.Processed, (await context.InboxMessages.SingleAsync()).Status);
        Assert.AreEqual(NotificationStatus.Sent, (await context.NotificationDispatches.SingleAsync()).Status);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Completed,
            (await context.NotificationDispatchDeliveries.SingleAsync()).Status);
        Assert.HasCount(1, gateway.Requests);
    }

    private static ServiceProvider CreateProvider(
        DispatchTestTimeProvider clock, RecordingGateway gateway,
        NotificationDispatchOptions? options = null, Action<IServiceCollection>? configure = null)
        => NotificationIntegrationTestHost.CreateServiceProvider(services =>
        {
            services.AddSingleton<IEmailGateway>(gateway);
            services.AddSingleton<ILogger<NotificationDispatchProcessor>, DispatchTestLogger>();
            configure?.Invoke(services);
        }, timeProvider: clock, dispatchOptions: options ?? CreateOptions());

    private static NotificationDispatchOptions CreateOptions() => new() { RetryJitterFactor = 0 };
    private static EmailSendResult Success() => new(true, "request", "provider-id", null, null);
    private static EmailSendResult Transient() => new(false, null, null, "temporary", "Try later", true);

    private sealed class RecordingGateway(Func<CancellationToken, Task<EmailSendResult>> send) : IEmailGateway
    {
        public string ProviderName => "Test";
        public ConcurrentQueue<EmailSendRequest> Requests { get; } = new();
        public Task<EmailSendResult> SendAsync(EmailSendRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Enqueue(request);
            return send(cancellationToken);
        }
    }

    private sealed class FailFirstAttemptSaveInterceptor : SaveChangesInterceptor
    {
        private int _failed;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<NotificationAttempt>()
                .Any(x => x.State == EntityState.Added) && Interlocked.Exchange(ref _failed, 1) == 0)
                throw new DbUpdateException("Simulated outcome persistence failure.");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    private sealed class DispatchTestLogger : ILogger<NotificationDispatchProcessor>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (exception is not null)
                Console.WriteLine($"{formatter(state, exception)}\n{exception}");
        }
    }
}

internal sealed class DispatchTestTimeProvider : TimeProvider
{
    public DateTime UtcNow { get; private set; } = new(2026, 10, 26, 12, 0, 0, DateTimeKind.Utc);
    public void Advance(TimeSpan delta) => UtcNow = UtcNow.Add(delta);
    public override DateTimeOffset GetUtcNow() => new(UtcNow);
}

internal static class NotificationDispatchTestData
{
    public static async Task ResetAsync()
    {
        await using var provider = NotificationIntegrationTestHost.CreateServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await db.Database.MigrateAsync();
        await db.InboxMessages.ExecuteDeleteAsync();
        await db.NotificationDispatches.ExecuteDeleteAsync();
        await db.NotificationTemplates.ExecuteDeleteAsync();
    }

    public static async Task<Guid> SeedAsync(ServiceProvider provider, DateTime now, bool enqueue = true)
    {
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var dispatch = NotificationDispatch.Create(Guid.NewGuid(), NotificationChannel.Email,
            NotificationTrigger.OrderCancelled, "Order", Guid.NewGuid(), null, "customer@example.com", "Customer");
        dispatch.SetRenderedContent("Subject", "Body");
        db.NotificationDispatches.Add(dispatch);
        if (enqueue)
            await scope.ServiceProvider.GetRequiredService<INotificationDispatchStore>().EnqueueAsync(dispatch.Id, now);
        await db.SaveChangesAsync();
        return dispatch.Id;
    }

    public static async Task<(NotificationDispatch Dispatch, NotificationDispatchDelivery Delivery)> LoadAsync(
        ServiceProvider provider, Guid id)
    {
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        return (await db.NotificationDispatches.AsNoTracking().Include(x => x.Attempts).SingleAsync(x => x.Id == id),
            await db.NotificationDispatchDeliveries.AsNoTracking().SingleAsync(x => x.Id == id));
    }
}
