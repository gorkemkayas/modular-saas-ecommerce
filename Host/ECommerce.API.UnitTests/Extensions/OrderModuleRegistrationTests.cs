using BuildingBlocks.Messaging.RabbitMQ.DependencyInjection;
using ECommerce.API.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Infrastructure.Outbox;
using Order.Infrastructure.Persistence.Outbox;
using BuildingBlocks.Infrastructure.Messaging.Inbox;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Consuming;
using Microsoft.Extensions.Options;
using Order.Infrastructure.Messaging.Shipment;
using Order.Infrastructure.Persistence;

namespace ECommerce.API.Extensions.UnitTests;

[TestClass]
public sealed class OrderModuleRegistrationTests
{
    [TestMethod]
    public async Task AddOrderModule_ResolvesOutboxWorkerAndScopedProcessor()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Modules:Order:Database:ConnectionString"] =
                    "Host=localhost;Database=registration_test;Username=test;Password=test",
                ["Modules:Order:IntegrationEventConsumer:QueueName"] = "test.order.events",
                ["Modules:Order:IntegrationEventConsumer:Retry:Delays:0"] = "00:00:01",
                ["Messaging:RabbitMq:HostName"] = "localhost",
                ["Messaging:RabbitMq:UserName"] = "registration-test",
                ["Messaging:RabbitMq:Password"] = "registration-test"
            }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRabbitMqMessaging(configuration);
        services.AddOrderModule(configuration);
        services.AddMediatR(options => options.RegisterServicesFromAssembly(
            typeof(Order.Application.AssemblyReference).Assembly));
        await using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true });

        // Resolve without starting the worker or opening external connections.
        var worker = provider.GetServices<IHostedService>()
            .OfType<OrderOutboxBackgroundService>().Single();
        await using var scope = provider.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<OrderOutboxProcessor>();
        var store = scope.ServiceProvider.GetRequiredService<IOrderOutboxStore>();

        Assert.IsNotNull(worker);
        Assert.IsNotNull(processor);
        Assert.IsInstanceOfType<OrderOutboxStore>(store);
        Assert.IsNotNull(provider.GetServices<IHostedService>()
            .OfType<RabbitMqIntegrationEventConsumer<OrderFulfillmentConsumerIdentity>>().Single());
        Assert.IsNotNull(provider.GetServices<IHostedService>()
            .OfType<ModuleInboxBackgroundService<OrderDbContext, OrderFulfillmentConsumerIdentity>>().Single());
        Assert.AreSame(provider.GetRequiredService<ModuleInboxProcessor<OrderDbContext, OrderFulfillmentConsumerIdentity>>(),
            scope.ServiceProvider.GetRequiredService<ModuleInboxProcessor<OrderDbContext, OrderFulfillmentConsumerIdentity>>());
        Assert.IsNotNull(scope.ServiceProvider.GetRequiredService<IIntegrationMessageReceiver<OrderFulfillmentConsumerIdentity>>());
        Assert.IsInstanceOfType<ShipmentFulfillmentInboxHandler>(scope.ServiceProvider
            .GetServices<IModuleInboxMessageHandler<OrderFulfillmentConsumerIdentity>>().Single());
        Assert.HasCount(1, provider.GetRequiredService<IOptions<RabbitMqConsumerOptions<OrderFulfillmentConsumerIdentity>>>()
            .Value.Retry.Delays);
    }
}
