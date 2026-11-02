using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace BuildingBlocks.Infrastructure.Messaging.Outbox;

public static class ModuleOutboxServiceCollectionExtensions
{
    public static IServiceCollection AddModuleOutbox<TContext, TIdentity>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TContext : Microsoft.EntityFrameworkCore.DbContext
        where TIdentity : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<ModuleOutboxOptions<TIdentity>>()
            .Bind(
                configuration.GetSection(
                    sectionName))
            .Validate(
                options =>
                    options.BatchSize is > 0 and <= 1000,
                "Module Outbox batch size must be between 1 and 1000.")
            .Validate(
                options =>
                    options.PollingInterval > TimeSpan.Zero,
                "Module Outbox polling interval must be greater than zero.")
            .Validate(
                options =>
                    options.ClaimDuration > TimeSpan.Zero,
                "Module Outbox claim duration must be greater than zero.")
            .Validate(
                options =>
                    options.MaximumAttempts > 0,
                "Module Outbox maximum attempts must be greater than zero.")
            .Validate(
                options =>
                    options.InitialRetryDelay > TimeSpan.Zero,
                "Module Outbox initial retry delay must be greater than zero.")
            .Validate(
                options =>
                    options.MaximumRetryDelay >=
                    options.InitialRetryDelay,
                "Module Outbox maximum retry delay must be greater than " +
                "or equal to the initial retry delay.")
            .Validate(options => options.RetryJitterFactor is >= 0 and <= 1,
                "Module Outbox retry jitter factor must be between 0 and 1.")
            .ValidateOnStart();

        services.TryAddSingleton<TimeProvider>(TimeProvider.System);

        services.AddScoped<IModuleOutboxStore<TContext>, ModuleOutboxStore<TContext>>();

        services.AddScoped<ModuleOutboxProcessor<TContext, TIdentity>>();

        services.AddHostedService<ModuleOutboxBackgroundService<TContext, TIdentity>>();

        return services;
    }
}
