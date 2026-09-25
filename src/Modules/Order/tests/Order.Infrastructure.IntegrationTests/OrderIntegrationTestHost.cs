using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Abstractions;
using Order.Application.Events;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.IntegrationTests;

internal static class OrderIntegrationTestHost
{
    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(
                typeof(Order.Application.AssemblyReference).Assembly);
        });

        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseNpgsql(
                PostgreSqlTestContainer.ConnectionString);
        });

        services.AddScoped<
            IOrderDomainEventDispatcher,
            OrderDomainEventDispatcher>();

        services.AddScoped<
            IOrderOutboxWriter,
            OrderOutboxWriter>();

        return services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
            });
    }
}