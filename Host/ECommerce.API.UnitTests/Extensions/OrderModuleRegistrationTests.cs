using BuildingBlocks.Messaging.RabbitMQ.DependencyInjection;
using ECommerce.API.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Infrastructure.Outbox;
using Order.Infrastructure.Persistence.Outbox;

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
    }
}
