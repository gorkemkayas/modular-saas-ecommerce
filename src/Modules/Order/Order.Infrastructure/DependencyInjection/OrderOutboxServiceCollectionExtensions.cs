using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Order.Infrastructure.Options;
using Order.Infrastructure.Outbox;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.DependencyInjection;

public static class OrderOutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOrderOutboxProcessing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<OrderOutboxOptions>()
            .Bind(
                configuration.GetSection(
                    OrderOutboxOptions.SectionName))
            .Validate(
                options =>
                    options.BatchSize is > 0 and <= 1000,
                "Order Outbox batch size must be between 1 and 1000.")
            .Validate(
                options =>
                    options.PollingInterval > TimeSpan.Zero,
                "Order Outbox polling interval must be greater than zero.")
            .Validate(
                options =>
                    options.ClaimDuration > TimeSpan.Zero,
                "Order Outbox claim duration must be greater than zero.")
            .Validate(
                options =>
                    options.MaximumAttempts > 0,
                "Order Outbox maximum attempts must be greater than zero.")
            .Validate(
                options =>
                    options.InitialRetryDelay > TimeSpan.Zero,
                "Order Outbox initial retry delay must be greater than zero.")
            .Validate(
                options =>
                    options.MaximumRetryDelay >=
                    options.InitialRetryDelay,
                "Order Outbox maximum retry delay must be greater than " +
                "or equal to the initial retry delay.")
            .ValidateOnStart();

        services.TryAddSingleton<TimeProvider>(TimeProvider.System);

        services.AddScoped<IOrderOutboxStore, OrderOutboxStore>();

        services.AddScoped<OrderOutboxProcessor>();

        services.AddHostedService<OrderOutboxBackgroundService>();

        return services;
    }
}
