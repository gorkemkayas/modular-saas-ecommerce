using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notification.Infrastructure.DependencyInjection;
using Notification.Infrastructure.Options;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class NotificationConsumerOptionsTests
{
    [TestMethod]
    public void Registration_ConfiguredRetryDelays_ReplaceDefaults()
    {
        var options = Resolve(new Dictionary<string, string?>
        {
            [$"{NotificationIntegrationEventConsumerOptions.SectionName}:Retry:Delays:0"] = "00:00:05",
            [$"{NotificationIntegrationEventConsumerOptions.SectionName}:Retry:Delays:1"] = "00:00:30"
        });

        CollectionAssert.AreEqual(
            new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30) }, options.Retry.Delays);
    }

    [TestMethod]
    public void Registration_WithoutConfiguredDelays_KeepsDefaults()
    {
        var options = Resolve(new Dictionary<string, string?>());

        CollectionAssert.AreEqual(
            new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30),
                TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(10) }, options.Retry.Delays);
    }

    [TestMethod]
    public void Registration_DescendingDelays_RejectsInvalidConfiguration()
    {
        Assert.ThrowsExactly<OptionsValidationException>(() => Resolve(
            new Dictionary<string, string?>
            {
                [$"{NotificationIntegrationEventConsumerOptions.SectionName}:Retry:Delays:0"] = "00:00:30",
                [$"{NotificationIntegrationEventConsumerOptions.SectionName}:Retry:Delays:1"] = "00:00:05"
            }));
    }

    private static NotificationIntegrationEventConsumerOptions Resolve(
        Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddNotificationInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<NotificationIntegrationEventConsumerOptions>>().Value;
    }
}
