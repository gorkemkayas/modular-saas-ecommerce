using BuildingBlocks.Infrastructure.Messaging.Outbox;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.DependencyInjection;
using ECommerce.API.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Payment.Infrastructure.Messaging;
using Payment.Infrastructure.Persistence;
using Shipment.Infrastructure.Messaging;
using Shipment.Infrastructure.Persistence;

namespace ECommerce.API.Extensions.UnitTests;

[TestClass]
public sealed class CommerceOutboxRegistrationTests
{
    [TestMethod]
    public async Task ModuleRegistration_IsolatesWorkersPublishersAndOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Modules:Payment:Database:ConnectionString"] = "Host=localhost;Database=test;Username=test;Password=test",
            ["Modules:Shipment:Database:ConnectionString"] = "Host=localhost;Database=test;Username=test;Password=test",
            ["Modules:Payment:Outbox:BatchSize"] = "7",
            ["Modules:Shipment:Outbox:BatchSize"] = "11",
            ["Messaging:RabbitMq:HostName"] = "localhost",
            ["Messaging:RabbitMq:UserName"] = "test",
            ["Messaging:RabbitMq:Password"] = "test"
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRabbitMqMessaging(configuration);
        services.AddPaymentModule(configuration);
        services.AddShipmentModule(configuration);
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        var workers = provider.GetServices<IHostedService>().ToArray();
        Assert.AreEqual(1, workers.OfType<ModuleOutboxBackgroundService<PaymentDbContext, PaymentOutboxPublisherIdentity>>().Count());
        Assert.AreEqual(1, workers.OfType<ModuleOutboxBackgroundService<ShipmentDbContext, ShipmentOutboxPublisherIdentity>>().Count());
        await using var scope = provider.CreateAsyncScope();
        Assert.IsNotNull(scope.ServiceProvider.GetRequiredService<ModuleOutboxProcessor<PaymentDbContext, PaymentOutboxPublisherIdentity>>());
        Assert.IsNotNull(scope.ServiceProvider.GetRequiredService<ModuleOutboxProcessor<ShipmentDbContext, ShipmentOutboxPublisherIdentity>>());
        Assert.AreEqual(7, provider.GetRequiredService<IOptions<ModuleOutboxOptions<PaymentOutboxPublisherIdentity>>>().Value.BatchSize);
        Assert.AreEqual(11, provider.GetRequiredService<IOptions<ModuleOutboxOptions<ShipmentOutboxPublisherIdentity>>>().Value.BatchSize);
        object paymentPublisher = provider.GetRequiredService<IIntegrationEventPublisher<PaymentOutboxPublisherIdentity>>();
        object shipmentPublisher = provider.GetRequiredService<IIntegrationEventPublisher<ShipmentOutboxPublisherIdentity>>();
        Assert.AreNotSame(paymentPublisher, shipmentPublisher);
    }
}
