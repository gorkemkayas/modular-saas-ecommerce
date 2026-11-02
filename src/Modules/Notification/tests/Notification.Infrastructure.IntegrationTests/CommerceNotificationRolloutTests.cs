using System.Text.Json;
using BuildingBlocks.Messaging.Abstractions.Events;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Retry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.Application.Notifications.Services;
using Notification.Domain.Enums;
using Notification.Infrastructure.Messaging.Dispatch;
using Notification.Infrastructure.Messaging.Inbox;
using Notification.Infrastructure.Messaging.RabbitMq;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Dispatch;
using Notification.Infrastructure.Persistence.Inbox;
using Notification.Infrastructure.Seeding;
using Order.Contracts.IntegrationEvents;
using Payment.Contracts.IntegrationEvents;
using Shipment.Contracts.IntegrationEvents;
using RabbitMQ.Client.Exceptions;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class CommerceNotificationRolloutTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [TestMethod]
    [DataRow(1, NotificationTrigger.OrderPlaced)]
    [DataRow(2, NotificationTrigger.PaymentAuthorized)]
    [DataRow(3, NotificationTrigger.PaymentCaptured)]
    [DataRow(4, NotificationTrigger.PaymentFailed)]
    [DataRow(5, NotificationTrigger.PaymentRefunded)]
    [DataRow(6, NotificationTrigger.ShipmentCreated)]
    [DataRow(7, NotificationTrigger.ShipmentShipped)]
    [DataRow(8, NotificationTrigger.ShipmentDelivered)]
    [DataRow(9, NotificationTrigger.ShipmentDeliveryException)]
    public async Task InboxToGateway_AllNewOutcomesCreateOneDurableDispatch(int scenario, NotificationTrigger trigger)
    {
        await NotificationDispatchTestData.ResetAsync();
        var gateway = new RecordingGateway();
        await using var provider = CreateProvider(gateway);
        await SeedDefaultsAsync(provider);
        var e = CreateEvent(scenario);
        await WriteInboxAsync(provider, e);

        await provider.GetRequiredService<NotificationInboxProcessor>().ProcessBatchAsync();
        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();
        Assert.AreEqual(InboxMessageWriteResult.Duplicate, await WriteInboxAsync(provider, e));
        await provider.GetRequiredService<NotificationInboxProcessor>().ProcessBatchAsync();
        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var dispatch = await db.NotificationDispatches.Include(x => x.Attempts).SingleAsync();
        Assert.AreEqual(trigger, dispatch.Trigger);
        Assert.AreEqual(NotificationStatus.Sent, dispatch.Status);
        Assert.HasCount(1, dispatch.Attempts);
        Assert.AreEqual(InboxMessageStatus.Processed, (await db.InboxMessages.SingleAsync()).Status);
        Assert.AreEqual(NotificationDispatchDeliveryStatus.Completed,
            (await db.NotificationDispatchDeliveries.SingleAsync()).Status);
        Assert.HasCount(1, gateway.Requests);
        if (scenario == 1)
        {
            StringAssert.Contains(gateway.Requests.Single().HtmlBody!, "Product snapshot");
            StringAssert.Contains(gateway.Requests.Single().HtmlBody!, "image.png");
        }
        if (scenario == 7)
            StringAssert.Contains(gateway.Requests.Single().HtmlBody!, "TRACK-1");
    }

    [TestMethod]
    public async Task PartialRefunds_SamePaymentCreatesSeparateDispatches()
    {
        await NotificationDispatchTestData.ResetAsync();
        var gateway = new RecordingGateway();
        await using var provider = CreateProvider(gateway);
        await SeedDefaultsAsync(provider);
        var first = (PaymentStatusChangedIntegrationEvent)CreateEvent(5);
        var second = first with { Id = Guid.NewGuid(), RefundId = Guid.NewGuid(), Amount = 30m };
        await WriteInboxAsync(provider, first);
        await WriteInboxAsync(provider, second);
        await provider.GetRequiredService<NotificationInboxProcessor>().ProcessBatchAsync();
        await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync();

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        Assert.AreEqual(2, await db.NotificationDispatches.CountAsync(x => x.BusinessEntityType == "PaymentRefund"));
        Assert.HasCount(2, gateway.Requests);
    }

    [TestMethod]
    public async Task BrokerTopology_RoutesEveryNewContractAndDeduplicatesRedelivery()
    {
        await NotificationDispatchTestData.ResetAsync();
        var gateway = new RecordingGateway();
        await using var provider = CreateProvider(gateway);
        await SeedDefaultsAsync(provider);
        var rabbit = RabbitMqTestContainer.CreateOptions("rollout.exchange." + Guid.NewGuid().ToString("N"));
        var consumerOptions = new NotificationIntegrationEventConsumerOptions
        {
            Enabled = true, QueueName = "rollout.queue." + Guid.NewGuid().ToString("N")
        };
        await using var connections = new RabbitMqConnectionProvider(OptionsFactory.Create(rabbit));
        await using var failurePublisher = new RabbitMqFailureMessagePublisher<NotificationFailurePublisherIdentity>(
            connections, OptionsFactory.Create(rabbit));
        var router = new NotificationIntegrationEventFailureRouter(failurePublisher, OptionsFactory.Create(consumerOptions),
            TimeProvider.System, NullLogger<NotificationIntegrationEventFailureRouter>.Instance);
        using var consumer = new NotificationIntegrationEventConsumer(connections,
            provider.GetRequiredService<IServiceScopeFactory>(), OptionsFactory.Create(rabbit), router,
            OptionsFactory.Create(consumerOptions), NullLogger<NotificationIntegrationEventConsumer>.Instance);
        await using var publisher = new RabbitMqIntegrationEventPublisher<RolloutPublisher>(connections, OptionsFactory.Create(rabbit));
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(25));
        await consumer.StartAsync(deadline.Token);
        try
        {
            await using var connection = await RabbitMqTestContainer.CreateConnectionAsync(deadline.Token);
            while (true)
            {
                await using var channel = await connection.CreateChannelAsync(cancellationToken: deadline.Token);
                try { await channel.QueueDeclarePassiveAsync(consumerOptions.QueueName, deadline.Token); break; }
                catch (OperationInterruptedException) { await Task.Delay(50, deadline.Token); }
            }

            var events = Enumerable.Range(1, 9).Select(CreateEvent).ToArray();
            foreach (var e in events)
            {
                var message = new IntegrationMessage(e.Id, e.EventType, e.EventType,
                    JsonSerializer.Serialize(e, e.GetType(), JsonOptions), e.OccurredOnUtc);
                await publisher.PublishAsync(message, deadline.Token);
                await publisher.PublishAsync(message, deadline.Token);
            }
            while (true)
            {
                await using var scope = provider.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                if (await db.InboxMessages.CountAsync(deadline.Token) == 9) break;
                await Task.Delay(50, deadline.Token);
            }

            await provider.GetRequiredService<NotificationInboxProcessor>().ProcessBatchAsync(deadline.Token);
            await provider.GetRequiredService<NotificationDispatchProcessor>().ProcessBatchAsync(deadline.Token);
            Assert.HasCount(9, gateway.Requests);
        }
        finally { await consumer.StopAsync(CancellationToken.None); }
    }

    private static ServiceProvider CreateProvider(RecordingGateway gateway) =>
        NotificationIntegrationTestHost.CreateServiceProvider(services => services.AddSingleton<IEmailGateway>(gateway));

    private static async Task SeedDefaultsAsync(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        await new NotificationTemplateSeeder(scope.ServiceProvider.GetRequiredService<NotificationDbContext>(),
            NullLogger<NotificationTemplateSeeder>.Instance).SeedAsync();
    }

    private static async Task<InboxMessageWriteResult> WriteInboxAsync(ServiceProvider provider, IIntegrationEvent e)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IInboxMessageWriter>().WriteAsync(new InboxMessage(
            e.Id, e.EventType, e.EventType, JsonSerializer.Serialize(e, e.GetType(), JsonOptions),
            e.OccurredOnUtc, DateTime.UtcNow));
    }

    private static IIntegrationEvent CreateEvent(int scenario)
    {
        var id = Guid.NewGuid(); var store = Guid.NewGuid(); var order = Guid.NewGuid(); var customer = Guid.NewGuid();
        var now = DateTime.UtcNow;
        if (scenario == 1) return new OrderPlacedIntegrationEvent(id, now, order, store, customer, "ORD-1",
            "customer@example.com", "Customer", "TRY", 100m, 10m, 110m,
            new[] { new OrderPlacedLine("Product snapshot", null, 1, 100m, "https://example.com/image.png") });
        if (scenario <= 5) return new PaymentStatusChangedIntegrationEvent(id, now, Guid.NewGuid(), store, order,
            customer, "ORD-1", (Payment.Contracts.IntegrationEvents.PaymentEventKind)(scenario - 1),
            100m, "TRY", "reference", "Payment description", "customer@example.com", "Customer",
            scenario == 5 ? Guid.NewGuid() : null);
        return new ShipmentStatusChangedIntegrationEvent(id, now, Guid.NewGuid(), store, order, customer,
            "ORD-1", "SHP-1", (Shipment.Contracts.IntegrationEvents.ShipmentEventKind)(scenario - 5),
            "customer@example.com", "Customer", "Carrier", "TRACK-1", "https://example.com/track", "Delivery description");
    }

    private sealed class RolloutPublisher { }
    private sealed class RecordingGateway : IEmailGateway
    {
        public string ProviderName => "Test";
        public List<EmailSendRequest> Requests { get; } = [];
        public Task<EmailSendResult> SendAsync(EmailSendRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(new EmailSendResult(true, "request", Guid.NewGuid().ToString(), null, null));
        }
    }
}
