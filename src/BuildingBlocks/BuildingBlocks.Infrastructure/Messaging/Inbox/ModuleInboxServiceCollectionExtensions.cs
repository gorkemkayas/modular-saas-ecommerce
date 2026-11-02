using BuildingBlocks.Messaging.Abstractions.Publishing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox;

public static class ModuleInboxServiceCollectionExtensions
{
    public static IServiceCollection AddModuleInbox<TContext, TIdentity>(
        this IServiceCollection services, IConfiguration configuration, string sectionName)
        where TContext : DbContext
        where TIdentity : class
    {
        services.AddOptions<ModuleInboxOptions<TIdentity>>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(x => x.BatchSize is > 0 and <= 1000, "Inbox batch size must be between 1 and 1000.")
            .Validate(x => x.PollingInterval > TimeSpan.Zero && x.ClaimDuration > TimeSpan.Zero,
                "Inbox polling interval and claim duration must be positive.")
            .Validate(x => x.MaximumAttempts is > 0 and <= 1000, "Inbox retry budget must be between 1 and 1000.")
            .Validate(x => x.InitialRetryDelay >= TimeSpan.FromMilliseconds(1) &&
                x.MaximumRetryDelay >= x.InitialRetryDelay, "Inbox retry delays must be positive and ordered.")
            .Validate(x => x.RetryJitterFactor is >= 0 and <= 1, "Inbox jitter must be between 0 and 1.")
            .ValidateOnStart();
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<IModuleInboxStore<TContext>, ModuleInboxStore<TContext>>();
        services.AddScoped<IIntegrationMessageReceiver<TIdentity>, ModuleInboxReceiver<TContext, TIdentity>>();
        services.AddSingleton<ModuleInboxProcessor<TContext, TIdentity>>();
        services.AddHostedService<ModuleInboxBackgroundService<TContext, TIdentity>>();
        return services;
    }
}
