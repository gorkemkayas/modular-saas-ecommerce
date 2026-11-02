using BuildingBlocks.Messaging.RabbitMQ.Consuming;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Messaging.RabbitMQ.DependencyInjection;

public static class RabbitMqConsumerServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqConsumer<TIdentity>(this IServiceCollection services,
        IConfiguration configuration, string sectionName, params string[] routingKeys) where TIdentity : class
    {
        services.AddOptions<RabbitMqConsumerOptions<TIdentity>>()
            .Configure(options =>
            {
                var section = configuration.GetSection(sectionName);
                section.Bind(options);
                // ConfigurationBinder appends to initialized arrays; explicitly replace configured delays.
                if (section.GetSection("Retry:Delays").Exists())
                    options.Retry.Delays = section.GetSection("Retry:Delays").Get<TimeSpan[]>() ?? [];
                options.RoutingKeys = routingKeys.ToArray();
            })
            .Validate(x => !x.Enabled || !string.IsNullOrWhiteSpace(x.QueueName), "Consumer queue name is required.")
            .Validate(x => x.PrefetchCount > 0, "Prefetch count must be positive.")
            .Validate(x => x.Retry.Delays.Length is > 0 and <= 100 &&
                x.Retry.Delays.All(d => d.TotalMilliseconds is >= 1 and <= int.MaxValue), "Retry delays are invalid.")
            .Validate(x => x.RoutingKeys.Length > 0, "Consumer bindings are required.")
            .ValidateOnStart();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<RabbitMqDeliveryFailureRouter<TIdentity>>();
        services.AddHostedService<RabbitMqIntegrationEventConsumer<TIdentity>>();
        return services;
    }
}
