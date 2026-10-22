using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.DependencyInjection;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using BuildingBlocks.Messaging.RabbitMQ.Retry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.RabbitMQ.IntegrationTests;

[TestClass]
public sealed class RabbitMqPublisherRegistrationTests
{
    [TestMethod]
    public async Task AddRabbitMqMessaging_WhenPublisherIdentitiesDiffer_RegistersIndependentSingletonPublishers()
    {
        IConfiguration configuration = CreateConfiguration();

        var services = new ServiceCollection();

        services.AddRabbitMqMessaging(configuration);

        await using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        var firstOrderPublisher = serviceProvider
            .GetRequiredService<
                IIntegrationEventPublisher<OrderPublisherIdentity>>();

        var secondOrderPublisher = serviceProvider
            .GetRequiredService<
                IIntegrationEventPublisher<OrderPublisherIdentity>>();

        var paymentPublisher = serviceProvider
            .GetRequiredService<
                IIntegrationEventPublisher<PaymentPublisherIdentity>>();

        Assert.AreSame(
            firstOrderPublisher,
            secondOrderPublisher);

        Assert.AreNotSame<object>(
            firstOrderPublisher,
            paymentPublisher);
    }

    [TestMethod]
    public async Task AddRabbitMqMessaging_WhenFailurePublisherIdentitiesDiffer_RegistersIndependentSingletonPublishers()
    {
        IConfiguration configuration = CreateConfiguration();

        var services = new ServiceCollection();

        services.AddRabbitMqMessaging(configuration);

        await using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        var firstOrderPublisher = serviceProvider
            .GetRequiredService<
                IRabbitMqFailureMessagePublisher<
                    OrderPublisherIdentity>>();

        var secondOrderPublisher = serviceProvider
            .GetRequiredService<
                IRabbitMqFailureMessagePublisher<
                    OrderPublisherIdentity>>();

        var paymentPublisher = serviceProvider
            .GetRequiredService<
                IRabbitMqFailureMessagePublisher<
                    PaymentPublisherIdentity>>();

        Assert.AreSame(
            firstOrderPublisher,
            secondOrderPublisher);

        Assert.AreNotSame<object>(
            firstOrderPublisher,
            paymentPublisher);
    }

    private static IConfiguration CreateConfiguration()
    {
        var values = new Dictionary<string, string?>
        {
            [$"{RabbitMqOptions.SectionName}:HostName"] =
                "localhost",
            [$"{RabbitMqOptions.SectionName}:Port"] =
                "5672",
            [$"{RabbitMqOptions.SectionName}:UserName"] =
                "test",
            [$"{RabbitMqOptions.SectionName}:Password"] =
                "test",
            [$"{RabbitMqOptions.SectionName}:VirtualHost"] =
                "/",
            [$"{RabbitMqOptions.SectionName}:PublishingClientProvidedName"] =
                "registration-tests-publishing",
            [$"{RabbitMqOptions.SectionName}:ConsumingClientProvidedName"] =
                "registration-tests-consuming",
            [$"{RabbitMqOptions.SectionName}:ExchangeName"] =
                "registration-tests.events",
            [$"{RabbitMqOptions.SectionName}:RequestedHeartbeat"] =
                "00:00:30",
            [$"{RabbitMqOptions.SectionName}:NetworkRecoveryInterval"] =
                "00:00:05",
            [$"{RabbitMqOptions.SectionName}:ConnectionTimeout"] =
                "00:00:10",
            [$"{RabbitMqOptions.SectionName}:PublishTimeout"] =
                "00:00:10"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private sealed class OrderPublisherIdentity
    {
    }

    private sealed class PaymentPublisherIdentity
    {
    }
}
