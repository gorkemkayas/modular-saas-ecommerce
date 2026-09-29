using BuildingBlocks.Messaging.Abstractions.Publishing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Order.Infrastructure.Options;
using Order.Infrastructure.Outbox;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.UnitTests.Outbox;

[TestClass]
public sealed class OrderOutboxProcessorTests
{
    [TestMethod]
    public async Task ProcessBatchAsync_WhenPublishSucceeds_MarksMessageAsPublished()
    {
        var now = new DateTimeOffset(
            2026,
            9,
            29,
            12,
            0,
            0,
            TimeSpan.Zero);

        var options = new OrderOutboxOptions
        {
            BatchSize = 25,
            ClaimDuration = TimeSpan.FromMinutes(3)
        };

        var claimedMessage = new ClaimedOutboxMessage(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.order-cancelled.v1",
            "order.order-cancelled.v1",
            """{"orderId":"123"}""",
            now.AddMinutes(-1).UtcDateTime,
            AttemptCount: 1,
            Guid.NewGuid());

        var outboxStore =
            new Mock<IOrderOutboxStore>(MockBehavior.Strict);

        outboxStore
            .Setup(store => store.ClaimAsync(
                It.Is<Guid>(lockId => lockId != Guid.Empty),
                now.UtcDateTime,
                now.Add(options.ClaimDuration).UtcDateTime,
                options.BatchSize,
                CancellationToken.None))
            .ReturnsAsync(new[] { claimedMessage });

        outboxStore
            .Setup(store => store.MarkPublishedAsync(
                claimedMessage.Id,
                claimedMessage.LockId,
                now.UtcDateTime,
                CancellationToken.None))
            .ReturnsAsync(true);

        IntegrationMessage? publishedMessage = null;

        var publisher =
            new Mock<IIntegrationEventPublisher>(
                MockBehavior.Strict);

        publisher
            .Setup(currentPublisher =>
                currentPublisher.PublishAsync(
                    It.IsAny<IntegrationMessage>(),
                    CancellationToken.None))
            .Callback<IntegrationMessage, CancellationToken>(
                (message, _) => publishedMessage = message)
            .Returns(Task.CompletedTask);

        var processor = new OrderOutboxProcessor(
            outboxStore.Object,
            publisher.Object,
            Microsoft.Extensions.Options.Options.Create(options),
            new FixedTimeProvider(now),
            NullLogger<OrderOutboxProcessor>.Instance);

        await processor.ProcessBatchAsync();

        Assert.IsNotNull(publishedMessage);
        Assert.AreEqual(
            claimedMessage.Id,
            publishedMessage.Id);
        Assert.AreEqual(
            claimedMessage.EventType,
            publishedMessage.EventType);
        Assert.AreEqual(
            claimedMessage.RoutingKey,
            publishedMessage.RoutingKey);
        Assert.AreEqual(
            claimedMessage.Payload,
            publishedMessage.Payload);
        Assert.AreEqual(
            claimedMessage.OccurredOnUtc,
            publishedMessage.OccurredOnUtc);

        outboxStore.Verify(
            store => store.ClaimAsync(
                It.Is<Guid>(lockId => lockId != Guid.Empty),
                now.UtcDateTime,
                now.Add(options.ClaimDuration).UtcDateTime,
                options.BatchSize,
                CancellationToken.None),
            Times.Once);

        publisher.Verify(
            currentPublisher =>
                currentPublisher.PublishAsync(
                    It.IsAny<IntegrationMessage>(),
                    CancellationToken.None),
            Times.Once);

        outboxStore.Verify(
            store => store.MarkPublishedAsync(
                claimedMessage.Id,
                claimedMessage.LockId,
                now.UtcDateTime,
                CancellationToken.None),
            Times.Once);

        outboxStore.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    [TestMethod]
    [DataRow(1, 5)]
    [DataRow(2, 10)]
    [DataRow(3, 20)]
    [DataRow(8, 300)]
    public async Task ProcessBatchAsync_WhenPublishFails_SchedulesRetryWithExpectedDelay(
        int attemptCount,
        int expectedDelaySeconds)
    {
        var now = new DateTimeOffset(
            2026,
            9,
            29,
            12,
            0,
            0,
            TimeSpan.Zero);

        var options = new OrderOutboxOptions
        {
            BatchSize = 25,
            ClaimDuration = TimeSpan.FromMinutes(3),
            MaximumAttempts = 10,
            InitialRetryDelay = TimeSpan.FromSeconds(5),
            MaximumRetryDelay = TimeSpan.FromMinutes(5)
        };

        var claimedMessage = new ClaimedOutboxMessage(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.order-cancelled.v1",
            "order.order-cancelled.v1",
            """{"orderId":"123"}""",
            now.AddMinutes(-1).UtcDateTime,
            attemptCount,
            Guid.NewGuid());

        var publishException =
            new InvalidOperationException(
                "Broker is unavailable.");

        var expectedNextAttemptAtUtc =
            now.AddSeconds(expectedDelaySeconds).UtcDateTime;

        var outboxStore =
            new Mock<IOrderOutboxStore>(
                MockBehavior.Strict);

        outboxStore
            .Setup(store => store.ClaimAsync(
                It.Is<Guid>(lockId => lockId != Guid.Empty),
                now.UtcDateTime,
                now.Add(options.ClaimDuration).UtcDateTime,
                options.BatchSize,
                CancellationToken.None))
            .ReturnsAsync(new[] { claimedMessage });

        outboxStore
            .Setup(store => store.ScheduleRetryAsync(
                claimedMessage.Id,
                claimedMessage.LockId,
                expectedNextAttemptAtUtc,
                publishException.ToString(),
                CancellationToken.None))
            .ReturnsAsync(true);

        var publisher =
            new Mock<IIntegrationEventPublisher>(
                MockBehavior.Strict);

        publisher
            .Setup(currentPublisher =>
                currentPublisher.PublishAsync(
                    It.IsAny<IntegrationMessage>(),
                    CancellationToken.None))
            .ThrowsAsync(publishException);

        var processor = new OrderOutboxProcessor(
            outboxStore.Object,
            publisher.Object,
            Microsoft.Extensions.Options.Options.Create(options),
            new FixedTimeProvider(now),
            NullLogger<OrderOutboxProcessor>.Instance);

        await processor.ProcessBatchAsync();

        outboxStore.Verify(
        store => store.ClaimAsync(
            It.Is<Guid>(lockId => lockId != Guid.Empty),
            now.UtcDateTime,
            now.Add(options.ClaimDuration).UtcDateTime,
            options.BatchSize,
            CancellationToken.None),
        Times.Once);

        publisher.Verify(
            currentPublisher =>
                currentPublisher.PublishAsync(
                    It.Is<IntegrationMessage>(
                        message =>
                            message.Id == claimedMessage.Id),
                    CancellationToken.None),
            Times.Once);

        outboxStore.Verify(
            store => store.ScheduleRetryAsync(
                claimedMessage.Id,
                claimedMessage.LockId,
                expectedNextAttemptAtUtc,
                publishException.ToString(),
                CancellationToken.None),
            Times.Once);

        outboxStore.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task ProcessBatchAsync_WhenMaximumAttemptsReached_MarksMessageAsFailed()
    {
        var now = new DateTimeOffset(
            2026,
            9,
            29,
            12,
            0,
            0,
            TimeSpan.Zero);

        var options = new OrderOutboxOptions
        {
            BatchSize = 25,
            ClaimDuration = TimeSpan.FromMinutes(3),
            MaximumAttempts = 3,
            InitialRetryDelay = TimeSpan.FromSeconds(5),
            MaximumRetryDelay = TimeSpan.FromMinutes(5)
        };

        var claimedMessage = new ClaimedOutboxMessage(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.order-cancelled.v1",
            "order.order-cancelled.v1",
            """{"orderId":"123"}""",
            now.AddMinutes(-1).UtcDateTime,
            AttemptCount: 3,
            Guid.NewGuid());

        var publishException =
            new InvalidOperationException(
                "Broker is unavailable.");

        var outboxStore =
            new Mock<IOrderOutboxStore>(
                MockBehavior.Strict);

        outboxStore
            .Setup(store => store.ClaimAsync(
                It.Is<Guid>(lockId => lockId != Guid.Empty),
                now.UtcDateTime,
                now.Add(options.ClaimDuration).UtcDateTime,
                options.BatchSize,
                CancellationToken.None))
            .ReturnsAsync(new[] { claimedMessage });

        outboxStore
            .Setup(store => store.MarkFailedAsync(
                claimedMessage.Id,
                claimedMessage.LockId,
                publishException.ToString(),
                CancellationToken.None))
            .ReturnsAsync(true);

        var publisher =
            new Mock<IIntegrationEventPublisher>(
                MockBehavior.Strict);

        publisher
            .Setup(currentPublisher =>
                currentPublisher.PublishAsync(
                    It.IsAny<IntegrationMessage>(),
                    CancellationToken.None))
            .ThrowsAsync(publishException);

        var processor = new OrderOutboxProcessor(
            outboxStore.Object,
            publisher.Object,
            Microsoft.Extensions.Options.Options.Create(options),
            new FixedTimeProvider(now),
            NullLogger<OrderOutboxProcessor>.Instance);

        await processor.ProcessBatchAsync();

        outboxStore.Verify(
            store => store.ClaimAsync(
                It.Is<Guid>(lockId => lockId != Guid.Empty),
                now.UtcDateTime,
                now.Add(options.ClaimDuration).UtcDateTime,
                options.BatchSize,
                CancellationToken.None),
            Times.Once);

        publisher.Verify(
            currentPublisher =>
                currentPublisher.PublishAsync(
                    It.Is<IntegrationMessage>(
                        message =>
                            message.Id == claimedMessage.Id),
                    CancellationToken.None),
            Times.Once);

        outboxStore.Verify(
            store => store.MarkFailedAsync(
                claimedMessage.Id,
                claimedMessage.LockId,
                publishException.ToString(),
                CancellationToken.None),
            Times.Once);

        outboxStore.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task ProcessBatchAsync_WhenApplicationIsStopping_PropagatesCancellation()
    {
        var now = new DateTimeOffset(
            2026,
            9,
            29,
            12,
            0,
            0,
            TimeSpan.Zero);

        var options = new OrderOutboxOptions
        {
            BatchSize = 25,
            ClaimDuration = TimeSpan.FromMinutes(3),
            MaximumAttempts = 10
        };

        var claimedMessage = new ClaimedOutboxMessage(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.order-cancelled.v1",
            "order.order-cancelled.v1",
            """{"orderId":"123"}""",
            now.AddMinutes(-1).UtcDateTime,
            AttemptCount: 1,
            Guid.NewGuid());

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var outboxStore =
            new Mock<IOrderOutboxStore>(
                MockBehavior.Strict);

        outboxStore
            .Setup(store => store.ClaimAsync(
                It.Is<Guid>(lockId => lockId != Guid.Empty),
                now.UtcDateTime,
                now.Add(options.ClaimDuration).UtcDateTime,
                options.BatchSize,
                cancellationToken))
            .ReturnsAsync(new[] { claimedMessage });

        var publisher =
            new Mock<IIntegrationEventPublisher>(
                MockBehavior.Strict);

        publisher
            .Setup(currentPublisher =>
                currentPublisher.PublishAsync(
                    It.IsAny<IntegrationMessage>(),
                    cancellationToken))
            .Callback<IntegrationMessage, CancellationToken>(
                (_, _) => cancellationTokenSource.Cancel())
            .ThrowsAsync(
                new OperationCanceledException(
                    cancellationToken));

        var processor = new OrderOutboxProcessor(
            outboxStore.Object,
            publisher.Object,
            Microsoft.Extensions.Options.Options.Create(options),
            new FixedTimeProvider(now),
            NullLogger<OrderOutboxProcessor>.Instance);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            () => processor.ProcessBatchAsync(
                cancellationToken));

        outboxStore.Verify(
            store => store.ClaimAsync(
                It.Is<Guid>(lockId => lockId != Guid.Empty),
                now.UtcDateTime,
                now.Add(options.ClaimDuration).UtcDateTime,
                options.BatchSize,
                cancellationToken),
            Times.Once);

        publisher.Verify(
            currentPublisher =>
                currentPublisher.PublishAsync(
                    It.IsAny<IntegrationMessage>(),
                    cancellationToken),
            Times.Once);

        outboxStore.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task ProcessBatchAsync_WhenOnePublishFails_ContinuesWithNextMessage()
    {
        var now = new DateTimeOffset(
            2026,
            9,
            29,
            12,
            0,
            0,
            TimeSpan.Zero);

        var options = new OrderOutboxOptions
        {
            BatchSize = 25,
            ClaimDuration = TimeSpan.FromMinutes(3),
            MaximumAttempts = 10,
            InitialRetryDelay = TimeSpan.FromSeconds(5),
            MaximumRetryDelay = TimeSpan.FromMinutes(5)
        };

        var claimedLockId = Guid.NewGuid();

        var firstMessage = new ClaimedOutboxMessage(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.first-event.v1",
            "order.first-event.v1",
            """{"sequence":1}""",
            now.AddMinutes(-2).UtcDateTime,
            AttemptCount: 1,
            claimedLockId);

        var secondMessage = new ClaimedOutboxMessage(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "order.second-event.v1",
            "order.second-event.v1",
            """{"sequence":2}""",
            now.AddMinutes(-1).UtcDateTime,
            AttemptCount: 1,
            claimedLockId);

        var publishException =
            new InvalidOperationException(
                "Broker rejected the first message.");

        var expectedNextAttemptAtUtc =
            now.Add(options.InitialRetryDelay).UtcDateTime;

        var outboxStore =
            new Mock<IOrderOutboxStore>(
                MockBehavior.Strict);

        outboxStore
            .Setup(store => store.ClaimAsync(
                It.Is<Guid>(lockId => lockId != Guid.Empty),
                now.UtcDateTime,
                now.Add(options.ClaimDuration).UtcDateTime,
                options.BatchSize,
                CancellationToken.None))
            .ReturnsAsync(new[]
            {
                firstMessage,
                secondMessage
            });

        outboxStore
            .Setup(store => store.ScheduleRetryAsync(
                firstMessage.Id,
                firstMessage.LockId,
                expectedNextAttemptAtUtc,
                publishException.ToString(),
                CancellationToken.None))
            .ReturnsAsync(true);

        outboxStore
            .Setup(store => store.MarkPublishedAsync(
                secondMessage.Id,
                secondMessage.LockId,
                now.UtcDateTime,
                CancellationToken.None))
            .ReturnsAsync(true);

        var publisher =
            new Mock<IIntegrationEventPublisher>(
                MockBehavior.Strict);

        publisher
            .Setup(currentPublisher =>
                currentPublisher.PublishAsync(
                    It.Is<IntegrationMessage>(
                        message =>
                            message.Id == firstMessage.Id),
                    CancellationToken.None))
            .ThrowsAsync(publishException);

        publisher
            .Setup(currentPublisher =>
                currentPublisher.PublishAsync(
                    It.Is<IntegrationMessage>(
                        message =>
                            message.Id == secondMessage.Id),
                    CancellationToken.None))
            .Returns(Task.CompletedTask);

        var processor = new OrderOutboxProcessor(
            outboxStore.Object,
            publisher.Object,
            Microsoft.Extensions.Options.Options.Create(options),
            new FixedTimeProvider(now),
            NullLogger<OrderOutboxProcessor>.Instance);

        await processor.ProcessBatchAsync();

        outboxStore.Verify(
            store => store.ClaimAsync(
                It.Is<Guid>(lockId => lockId != Guid.Empty),
                now.UtcDateTime,
                now.Add(options.ClaimDuration).UtcDateTime,
                options.BatchSize,
                CancellationToken.None),
            Times.Once);

        publisher.Verify(
            currentPublisher =>
                currentPublisher.PublishAsync(
                    It.Is<IntegrationMessage>(
                        message =>
                            message.Id == firstMessage.Id),
                    CancellationToken.None),
            Times.Once);

        publisher.Verify(
            currentPublisher =>
                currentPublisher.PublishAsync(
                    It.Is<IntegrationMessage>(
                        message =>
                            message.Id == secondMessage.Id),
                    CancellationToken.None),
            Times.Once);

        outboxStore.Verify(
            store => store.ScheduleRetryAsync(
                firstMessage.Id,
                firstMessage.LockId,
                expectedNextAttemptAtUtc,
                publishException.ToString(),
                CancellationToken.None),
            Times.Once);

        outboxStore.Verify(
            store => store.MarkPublishedAsync(
                secondMessage.Id,
                secondMessage.LockId,
                now.UtcDateTime,
                CancellationToken.None),
            Times.Once);

        outboxStore.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}