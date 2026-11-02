using BuildingBlocks.Infrastructure.Messaging.Outbox;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using Microsoft.Extensions.Logging.Abstractions;
using OptionsFactory = Microsoft.Extensions.Options.Options;
using Moq;
using Order.Infrastructure.Options;
using Order.Infrastructure.Outbox;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.UnitTests.Outbox;

[TestClass]
public sealed class OrderOutboxOwnershipTests
{
    [TestMethod]
    public async Task LostLease_IsNotPublished()
    {
        var test = new Fixture();
        test.Store.Setup(x => x.RenewLeaseAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(),
            It.IsAny<DateTime>(), CancellationToken.None)).ReturnsAsync(false);
        await test.Processor().ProcessBatchAsync();
        test.Publisher.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task RecoveredClaimBeyondBudget_IsFailedWithoutPublishing()
    {
        var test = new Fixture();
        test.Messages = [test.Message with { AttemptCount = 11 }];
        await test.Processor().ProcessBatchAsync();
        test.Publisher.VerifyNoOtherCalls();
        test.Store.Verify(x => x.MarkFailedAsync(test.Message.Id, test.Message.LockId,
            It.IsAny<string>(), CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public async Task EachMessage_RenewsUsingCurrentTimeRatherThanBatchStart()
    {
        var test = new Fixture();
        var next = test.Message with { Id = Guid.NewGuid() };
        test.Messages = [test.Message, next];
        test.Publisher.Setup(x => x.PublishAsync(It.IsAny<IntegrationMessage>(), CancellationToken.None))
            .Callback(() => test.Clock.Now = test.Clock.Now.AddSeconds(30)).Returns(Task.CompletedTask);
        DateTime secondStart = test.Clock.Now.AddSeconds(30);
        await test.Processor().ProcessBatchAsync();
        test.Store.Verify(x => x.RenewLeaseAsync(next.Id, next.LockId, secondStart, secondStart.AddMinutes(2),
            CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public async Task RetryDelay_UsesConfiguredJitterBounds()
    {
        var test = new Fixture();
        test.Publisher.Setup(x => x.PublishAsync(It.IsAny<IntegrationMessage>(), CancellationToken.None))
            .ThrowsAsync(new IOException("Broker unavailable"));
        DateTime now = test.Clock.Now;
        await test.Processor().ProcessBatchAsync();
        test.Store.Verify(x => x.ScheduleRetryAsync(test.Message.Id, test.Message.LockId,
            It.Is<DateTime>(time => time >= now.AddSeconds(4) && time <= now.AddSeconds(6)),
            It.IsAny<string>(), CancellationToken.None), Times.Once);
    }

    private sealed class Fixture
    {
        public Mock<IOrderOutboxStore> Store { get; } = new();
        public Mock<IIntegrationEventPublisher<OrderOutboxPublisherIdentity>> Publisher { get; } = new(MockBehavior.Strict);
        public Clock Clock { get; } = new();
        public ClaimedOutboxMessage Message { get; } = new(Guid.NewGuid(), Guid.NewGuid(), "test.v1", "test.v1", "{}",
            DateTime.UtcNow, 1, Guid.NewGuid());
        public IReadOnlyList<ClaimedOutboxMessage> Messages { get; set; }
        public Fixture()
        {
            Messages = [Message];
            Store.Setup(x => x.ClaimAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(() => Messages);
            Store.Setup(x => x.RenewLeaseAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), CancellationToken.None)).ReturnsAsync(true);
        }
        public OrderOutboxProcessor Processor() => new(Store.Object, Publisher.Object, OptionsFactory.Create(new OrderOutboxOptions()),
            Clock, NullLogger<ModuleOutboxProcessor<OrderDbContext, OrderOutboxPublisherIdentity>>.Instance);
    }
    private sealed class Clock : TimeProvider
    {
        public DateTime Now { get; set; } = DateTime.UtcNow;
        public override DateTimeOffset GetUtcNow() => new(Now);
    }
}
