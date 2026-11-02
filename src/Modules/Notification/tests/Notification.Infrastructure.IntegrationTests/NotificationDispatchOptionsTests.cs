using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Notification.Infrastructure.DependencyInjection;
using Notification.Infrastructure.Messaging.Dispatch;
using Notification.Infrastructure.Options;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class NotificationDispatchOptionsTests
{
    [TestMethod]
    [DataRow("BatchSize", "0")]
    [DataRow("MaximumAttempts", "0")]
    [DataRow("ClaimDuration", "00:00:10")]
    [DataRow("SendTimeout", "00:00:00")]
    [DataRow("RetryJitterFactor", "1.1")]
    [DataRow("MaximumDeliveryAge", "24:00:00")]
    public void Registration_RejectsInvalidDispatchSettings(string key, string value)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { [$"{NotificationDispatchOptions.SectionName}:{key}"] = value }).Build();
        var services = new ServiceCollection();
        services.AddNotificationInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.ThrowsExactly<OptionsValidationException>(() =>
        {
            _ = provider.GetRequiredService<IOptions<NotificationDispatchOptions>>().Value;
        });
    }

    [TestMethod]
    public async Task BackgroundService_WhenDisabled_DoesNotClaimOrSend()
    {
        var clock = new DispatchTestTimeProvider();
        await using var provider = NotificationIntegrationTestHost.CreateServiceProvider(timeProvider: clock);
        var options = Microsoft.Extensions.Options.Options.Create(new NotificationDispatchOptions { Enabled = false });
        using var service = new NotificationDispatchBackgroundService(
            provider.GetRequiredService<NotificationDispatchProcessor>(), options, clock,
            NullLogger<NotificationDispatchBackgroundService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await service.ExecuteTask!;
        await service.StopAsync(CancellationToken.None);
    }
}
