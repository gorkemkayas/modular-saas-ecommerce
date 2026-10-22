using System.Collections.Concurrent;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using BuildingBlocks.Messaging.RabbitMQ.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Retry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notification.Infrastructure.Messaging.RabbitMq;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Inbox;
using Order.Contracts.IntegrationEvents;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class NotificationIntegrationEventConsumerTests
{
    [TestMethod]
    public async Task Delivery_WhenMessageIsValid_PersistsAndAcknowledgesMessage()
    {
        var recorder = new InboxWriteResultRecorder();

        await using ServiceProvider serviceProvider =
            CreateServiceProvider(recorder);

        await MigrateDatabaseAsync(serviceProvider);

        string exchangeName = CreateUniqueName("exchange");
        string queueName = CreateUniqueName("queue");
        RabbitMqOptions rabbitMqOptions =
            RabbitMqTestContainer.CreateOptions(exchangeName);

        await using var connectionProvider =
            new RabbitMqConnectionProvider(
                OptionsFactory.Create(rabbitMqOptions));

        var logger = new ConsumerTestLogger();
        await using ConsumerHarness harness =
            CreateConsumer(
                connectionProvider,
                serviceProvider,
                rabbitMqOptions,
                queueName,
                logger);

        NotificationIntegrationEventConsumer consumer =
            harness.Consumer;

        using var timeoutSource =
            new CancellationTokenSource(TimeSpan.FromSeconds(20));

        try
        {
            await consumer.StartAsync(timeoutSource.Token);
            await WaitForQueueAsync(queueName, timeoutSource.Token);

            Guid messageId = Guid.NewGuid();

            await PublishAsync(
                connectionProvider,
                rabbitMqOptions,
                CreateMessage(messageId),
                timeoutSource.Token);

            await logger.WaitForAcknowledgementsAsync(
                expectedCount: 1,
                timeoutSource.Token);

            CollectionAssert.AreEqual(
                new[] { InboxMessageWriteResult.Inserted },
                recorder.Results.ToArray());

            Assert.AreEqual(
                1,
                await CountInboxMessagesAsync(
                    serviceProvider,
                    messageId));
        }
        finally
        {
            await consumer.StopAsync(CancellationToken.None);
        }

        await AssertQueueIsEmptyAsync(
            queueName,
            CancellationToken.None);
    }

    [TestMethod]
    public async Task Delivery_WhenMessageIsDeliveredTwice_AcknowledgesBothAndPersistsOneRow()
    {
        var recorder = new InboxWriteResultRecorder();

        await using ServiceProvider serviceProvider =
            CreateServiceProvider(recorder);

        await MigrateDatabaseAsync(serviceProvider);

        string exchangeName = CreateUniqueName("exchange");
        string queueName = CreateUniqueName("queue");
        RabbitMqOptions rabbitMqOptions =
            RabbitMqTestContainer.CreateOptions(exchangeName);

        await using var connectionProvider =
            new RabbitMqConnectionProvider(
                OptionsFactory.Create(rabbitMqOptions));

        var logger = new ConsumerTestLogger();
        await using ConsumerHarness harness =
            CreateConsumer(
                connectionProvider,
                serviceProvider,
                rabbitMqOptions,
                queueName,
                logger);

        NotificationIntegrationEventConsumer consumer =
            harness.Consumer;

        using var timeoutSource =
            new CancellationTokenSource(TimeSpan.FromSeconds(20));

        Guid messageId = Guid.NewGuid();
        IntegrationMessage message = CreateMessage(messageId);

        try
        {
            await consumer.StartAsync(timeoutSource.Token);
            await WaitForQueueAsync(queueName, timeoutSource.Token);

            await PublishAsync(
                connectionProvider,
                rabbitMqOptions,
                message,
                timeoutSource.Token);

            await PublishAsync(
                connectionProvider,
                rabbitMqOptions,
                message,
                timeoutSource.Token);

            await logger.WaitForAcknowledgementsAsync(
                expectedCount: 2,
                timeoutSource.Token);

            InboxMessageWriteResult[] results =
                recorder.Results.ToArray();

            Assert.HasCount(2, results);
            Assert.AreEqual(
                1,
                results.Count(
                    result =>
                        result == InboxMessageWriteResult.Inserted));
            Assert.AreEqual(
                1,
                results.Count(
                    result =>
                        result == InboxMessageWriteResult.Duplicate));

            Assert.AreEqual(
                1,
                await CountInboxMessagesAsync(
                    serviceProvider,
                    messageId));
        }
        finally
        {
            await consumer.StopAsync(CancellationToken.None);
        }

        await AssertQueueIsEmptyAsync(
            queueName,
            CancellationToken.None);
    }

    [TestMethod]
    public async Task Delivery_WhenPayloadIsInvalid_RoutesDirectlyToDeadLetterQueue()
    {
        var recorder = new InboxWriteResultRecorder();

        await using ServiceProvider serviceProvider =
            CreateServiceProvider(recorder);

        await MigrateDatabaseAsync(serviceProvider);

        string exchangeName = CreateUniqueName("exchange");
        string queueName = CreateUniqueName("queue");
        string deadLetterQueueName = $"{queueName}.dead-letter.queue";

        RabbitMqOptions rabbitMqOptions =
            RabbitMqTestContainer.CreateOptions(exchangeName);

        await using var connectionProvider =
            new RabbitMqConnectionProvider(
                OptionsFactory.Create(rabbitMqOptions));

        var logger = new ConsumerTestLogger();
        await using ConsumerHarness harness =
            CreateConsumer(
                connectionProvider,
                serviceProvider,
                rabbitMqOptions,
                queueName,
                logger);

        using var timeoutSource =
            new CancellationTokenSource(TimeSpan.FromSeconds(20));

        Guid messageId = Guid.NewGuid();
        var invalidMessage = new IntegrationMessage(
            messageId,
            OrderCancelledIntegrationEvent.EventType,
            OrderCancelledIntegrationEvent.EventType,
            "not-json",
            DateTime.UtcNow);

        try
        {
            await harness.Consumer.StartAsync(timeoutSource.Token);
            await WaitForQueueAsync(queueName, timeoutSource.Token);
            await WaitForQueueAsync(deadLetterQueueName, timeoutSource.Token);

            await PublishAsync(
                connectionProvider,
                rabbitMqOptions,
                invalidMessage,
                timeoutSource.Token);

            BasicGetResult delivery =
                await WaitForMessageAsync(
                    deadLetterQueueName,
                    timeoutSource.Token);

            Assert.AreEqual(
                messageId.ToString("D"),
                delivery.BasicProperties.MessageId);
            Assert.AreEqual(
                "not-json",
                System.Text.Encoding.UTF8.GetString(
                    delivery.Body.Span));

            RabbitMqRetryMetadata metadata =
                RabbitMqRetryMetadataCodec.Read(
                    delivery.BasicProperties.Headers!);

            Assert.AreEqual(0, metadata.RetryAttempt);
            Assert.AreEqual(exchangeName, metadata.OriginalExchange);
            Assert.AreEqual(
                OrderCancelledIntegrationEvent.EventType,
                metadata.OriginalRoutingKey);
            Assert.AreEqual(
                0,
                await CountInboxMessagesAsync(
                    serviceProvider,
                    messageId));
        }
        finally
        {
            await harness.Consumer.StopAsync(CancellationToken.None);
        }
    }

    [TestMethod]
    public async Task Delivery_WhenRetryMetadataIsIncomplete_ReplacesMetadataAndRoutesToDeadLetterQueue()
    {
        var recorder = new InboxWriteResultRecorder();

        await using ServiceProvider serviceProvider =
            CreateServiceProvider(recorder);

        await MigrateDatabaseAsync(serviceProvider);

        string exchangeName = CreateUniqueName("exchange");
        string queueName = CreateUniqueName("queue");
        string deadLetterQueueName = $"{queueName}.dead-letter.queue";

        RabbitMqOptions rabbitMqOptions =
            RabbitMqTestContainer.CreateOptions(exchangeName);

        await using var connectionProvider =
            new RabbitMqConnectionProvider(
                OptionsFactory.Create(rabbitMqOptions));

        var logger = new ConsumerTestLogger();
        await using ConsumerHarness harness =
            CreateConsumer(
                connectionProvider,
                serviceProvider,
                rabbitMqOptions,
                queueName,
                logger);

        using var timeoutSource =
            new CancellationTokenSource(TimeSpan.FromSeconds(20));

        Guid messageId = Guid.NewGuid();
        IntegrationMessage message = CreateMessage(messageId);

        try
        {
            await harness.Consumer.StartAsync(timeoutSource.Token);
            await WaitForQueueAsync(queueName, timeoutSource.Token);
            await WaitForQueueAsync(
                deadLetterQueueName,
                timeoutSource.Token);

            await PublishWithIncompleteRetryMetadataAsync(
                rabbitMqOptions,
                message,
                timeoutSource.Token);

            BasicGetResult delivery =
                await WaitForMessageAsync(
                    deadLetterQueueName,
                    timeoutSource.Token);

            RabbitMqRetryMetadata metadata =
                RabbitMqRetryMetadataCodec.Read(
                    delivery.BasicProperties.Headers!);

            Assert.AreEqual(0, metadata.RetryAttempt);
            Assert.AreEqual(exchangeName, metadata.OriginalExchange);
            Assert.AreEqual(message.RoutingKey, metadata.OriginalRoutingKey);
            Assert.AreEqual(
                messageId.ToString("D"),
                delivery.BasicProperties.MessageId);
            Assert.AreEqual(
                0,
                await CountInboxMessagesAsync(
                    serviceProvider,
                    messageId));
        }
        finally
        {
            await harness.Consumer.StopAsync(CancellationToken.None);
        }
    }

    [TestMethod]
    public async Task Delivery_WhenInboxPersistenceKeepsFailing_RetriesThenRoutesToDeadLetterQueue()
    {
        var recorder = new InboxWriteResultRecorder();

        await using ServiceProvider serviceProvider =
            CreateServiceProvider(
                recorder,
                services =>
                    services.AddScoped<
                        IInboxMessageWriter,
                        AlwaysFailingInboxMessageWriter>());

        await MigrateDatabaseAsync(serviceProvider);

        string exchangeName = CreateUniqueName("exchange");
        string queueName = CreateUniqueName("queue");
        string deadLetterQueueName = $"{queueName}.dead-letter.queue";

        RabbitMqOptions rabbitMqOptions =
            RabbitMqTestContainer.CreateOptions(exchangeName);

        await using var connectionProvider =
            new RabbitMqConnectionProvider(
                OptionsFactory.Create(rabbitMqOptions));

        var logger = new ConsumerTestLogger();
        await using ConsumerHarness harness =
            CreateConsumer(
                connectionProvider,
                serviceProvider,
                rabbitMqOptions,
                queueName,
                logger,
                retryDelays:
                [
                    TimeSpan.FromMilliseconds(200)
                ]);

        using var timeoutSource =
            new CancellationTokenSource(TimeSpan.FromSeconds(20));

        Guid messageId = Guid.NewGuid();
        IntegrationMessage message = CreateMessage(messageId);

        try
        {
            await harness.Consumer.StartAsync(timeoutSource.Token);
            await WaitForQueueAsync(queueName, timeoutSource.Token);
            await WaitForQueueAsync(deadLetterQueueName, timeoutSource.Token);

            await PublishAsync(
                connectionProvider,
                rabbitMqOptions,
                message,
                timeoutSource.Token);

            BasicGetResult delivery =
                await WaitForMessageAsync(
                    deadLetterQueueName,
                    timeoutSource.Token);

            RabbitMqRetryMetadata metadata =
                RabbitMqRetryMetadataCodec.Read(
                    delivery.BasicProperties.Headers!);

            Assert.AreEqual(2, metadata.RetryAttempt);
            Assert.AreEqual(exchangeName, metadata.OriginalExchange);
            Assert.AreEqual(
                OrderCancelledIntegrationEvent.EventType,
                metadata.OriginalRoutingKey);
            Assert.AreEqual(
                messageId.ToString("D"),
                delivery.BasicProperties.MessageId);
            Assert.AreEqual(
                message.Payload,
                System.Text.Encoding.UTF8.GetString(
                    delivery.Body.Span));
            Assert.AreEqual(
                0,
                await CountInboxMessagesAsync(
                    serviceProvider,
                    messageId));
        }
        finally
        {
            await harness.Consumer.StopAsync(CancellationToken.None);
        }
    }

    private static ServiceProvider CreateServiceProvider(
        InboxWriteResultRecorder recorder,
        Action<IServiceCollection>? configureServices = null)
    {
        return NotificationIntegrationTestHost.CreateServiceProvider(
            services =>
            {
                services.AddSingleton(recorder);
                services.AddScoped<InboxMessageWriter>();
                services.AddScoped<IInboxMessageWriter>(
                    serviceProvider =>
                        new RecordingInboxMessageWriter(
                            serviceProvider.GetRequiredService<
                                InboxMessageWriter>(),
                            serviceProvider.GetRequiredService<
                                InboxWriteResultRecorder>()));

                configureServices?.Invoke(services);
            });
    }

    private static ConsumerHarness CreateConsumer(
        IRabbitMqConnectionProvider connectionProvider,
        ServiceProvider serviceProvider,
        RabbitMqOptions rabbitMqOptions,
        string queueName,
        ILogger<NotificationIntegrationEventConsumer> logger,
        TimeSpan[]? retryDelays = null)
    {
        var consumerOptions =
            new NotificationIntegrationEventConsumerOptions
            {
                Enabled = true,
                QueueName = queueName,
                PrefetchCount = 10,
                Retry = new RabbitMqRetryOptions
                {
                    Delays = retryDelays ??
                    [
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromMinutes(2),
                        TimeSpan.FromMinutes(10)
                    ]
                }
            };

        var failurePublisher =
            new RabbitMqFailureMessagePublisher<
                NotificationFailurePublisherIdentity>(
                    connectionProvider,
                    OptionsFactory.Create(rabbitMqOptions));

        var failureRouter =
            new NotificationIntegrationEventFailureRouter(
                failurePublisher,
                OptionsFactory.Create(consumerOptions),
                TimeProvider.System,
                serviceProvider.GetRequiredService<
                    ILogger<NotificationIntegrationEventFailureRouter>>());

        var consumer = new NotificationIntegrationEventConsumer(
            connectionProvider,
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            OptionsFactory.Create(rabbitMqOptions),
            failureRouter,
            OptionsFactory.Create(consumerOptions),
            logger);

        return new ConsumerHarness(
            consumer,
            failurePublisher);
    }

    private static async Task PublishAsync(
        IRabbitMqConnectionProvider connectionProvider,
        RabbitMqOptions rabbitMqOptions,
        IntegrationMessage message,
        CancellationToken cancellationToken)
    {
        await using var publisher =
            new RabbitMqIntegrationEventPublisher<
                NotificationConsumerTestPublisherIdentity>(
                    connectionProvider,
                    OptionsFactory.Create(rabbitMqOptions));

        await publisher.PublishAsync(
            message,
            cancellationToken);
    }

    private static async Task PublishWithIncompleteRetryMetadataAsync(
        RabbitMqOptions rabbitMqOptions,
        IntegrationMessage message,
        CancellationToken cancellationToken)
    {
        await using IConnection connection =
            await RabbitMqTestContainer.CreateConnectionAsync(
                cancellationToken);
        await using IChannel channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            MessageId = message.Id.ToString("D"),
            Type = message.EventType,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(
                new DateTimeOffset(message.OccurredOnUtc)
                    .ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>
            {
                [RabbitMqRetryHeaders.RetryAttempt] = 1
            }
        };

        await channel.BasicPublishAsync(
            exchange: rabbitMqOptions.ExchangeName,
            routingKey: message.RoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: System.Text.Encoding.UTF8.GetBytes(message.Payload),
            cancellationToken: cancellationToken);
    }

    private static IntegrationMessage CreateMessage(Guid messageId)
    {
        return new IntegrationMessage(
            messageId,
            OrderCancelledIntegrationEvent.EventType,
            OrderCancelledIntegrationEvent.EventType,
            """{"orderId":"7d3cbce7-40f0-4e65-a18f-894da6752435"}""",
            DateTime.UtcNow);
    }

    private static async Task WaitForQueueAsync(
        string queueName,
        CancellationToken cancellationToken)
    {
        await using IConnection connection =
            await RabbitMqTestContainer.CreateConnectionAsync(
                cancellationToken);

        while (true)
        {
            await using IChannel channel =
                await connection.CreateChannelAsync(
                    cancellationToken: cancellationToken);

            try
            {
                await channel.QueueDeclarePassiveAsync(
                    queueName,
                    cancellationToken);

                return;
            }
            catch (OperationInterruptedException)
            {
                await Task.Delay(
                    TimeSpan.FromMilliseconds(50),
                    cancellationToken);
            }
        }
    }

    private static async Task AssertQueueIsEmptyAsync(
        string queueName,
        CancellationToken cancellationToken)
    {
        await using IConnection connection =
            await RabbitMqTestContainer.CreateConnectionAsync(
                cancellationToken);
        await using IChannel channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        BasicGetResult? delivery = await channel.BasicGetAsync(
            queueName,
            autoAck: false,
            cancellationToken);

        Assert.IsNull(
            delivery,
            "The consumer stopped with an unacknowledged message.");
    }

    private static async Task<BasicGetResult> WaitForMessageAsync(
        string queueName,
        CancellationToken cancellationToken)
    {
        await using IConnection connection =
            await RabbitMqTestContainer.CreateConnectionAsync(
                cancellationToken);
        await using IChannel channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        while (true)
        {
            BasicGetResult? delivery =
                await channel.BasicGetAsync(
                    queueName,
                    autoAck: true,
                    cancellationToken);

            if (delivery is not null)
            {
                return delivery;
            }

            await Task.Delay(
                TimeSpan.FromMilliseconds(50),
                cancellationToken);
        }
    }

    private static async Task<int> CountInboxMessagesAsync(
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
            .CountAsync(message => message.Id == messageId);
    }

    private static async Task MigrateDatabaseAsync(
        ServiceProvider serviceProvider)
    {
        await using AsyncServiceScope scope =
            serviceProvider.CreateAsyncScope();

        NotificationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<
                NotificationDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    private static string CreateUniqueName(string prefix)
    {
        return $"notification-tests.{prefix}.{Guid.NewGuid():N}";
    }

    private sealed class NotificationConsumerTestPublisherIdentity
    {
    }

    private sealed class ConsumerHarness(
        NotificationIntegrationEventConsumer consumer,
        RabbitMqFailureMessagePublisher<
            NotificationFailurePublisherIdentity> failurePublisher)
        : IAsyncDisposable
    {
        public NotificationIntegrationEventConsumer Consumer { get; } =
            consumer;

        public async ValueTask DisposeAsync()
        {
            Consumer.Dispose();
            await failurePublisher.DisposeAsync();
        }
    }

    private sealed class InboxWriteResultRecorder
    {
        public ConcurrentQueue<InboxMessageWriteResult> Results { get; } = [];
    }

    private sealed class RecordingInboxMessageWriter(
        InboxMessageWriter innerWriter,
        InboxWriteResultRecorder recorder)
        : IInboxMessageWriter
    {
        public async Task<InboxMessageWriteResult> WriteAsync(
            InboxMessage message,
            CancellationToken cancellationToken = default)
        {
            InboxMessageWriteResult result =
                await innerWriter.WriteAsync(
                    message,
                    cancellationToken);

            recorder.Results.Enqueue(result);

            return result;
        }
    }

    private sealed class AlwaysFailingInboxMessageWriter
        : IInboxMessageWriter
    {
        public Task<InboxMessageWriteResult> WriteAsync(
            InboxMessage message,
            CancellationToken cancellationToken = default)
        {
            throw new TimeoutException(
                "Simulated transient inbox persistence failure.");
        }
    }

    private sealed class ConsumerTestLogger
        : ILogger<NotificationIntegrationEventConsumer>
    {
        private int _acknowledgementCount;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            string message = formatter(state, exception);

            if (logLevel == LogLevel.Information &&
                message.Contains(
                    "persisted and acknowledged",
                    StringComparison.Ordinal))
            {
                Interlocked.Increment(
                    ref _acknowledgementCount);
            }
        }

        public async Task WaitForAcknowledgementsAsync(
            int expectedCount,
            CancellationToken cancellationToken)
        {
            while (Volatile.Read(ref _acknowledgementCount) <
                   expectedCount)
            {
                await Task.Delay(
                    TimeSpan.FromMilliseconds(25),
                    cancellationToken);
            }
        }
    }
}
