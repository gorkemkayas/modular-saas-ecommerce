using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Subscription.Application.Abstractions;
using Subscription.Application.Contracts;
using Subscription.Contracts;
using Subscription.Domain.Repositories;
using Subscription.Infrastructure.Gateways;
using Subscription.Infrastructure.Options;
using Subscription.Infrastructure.Persistence;
using Subscription.Infrastructure.Persistence.Repositories;
using Subscription.Infrastructure.Seeding;

namespace Subscription.Infrastructure.DependencyInjection;

public static class SubscriptionInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSubscriptionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SubscriptionDatabaseOptions>(
            configuration.GetSection(SubscriptionDatabaseOptions.SectionName));

        services.AddDbContext<SubscriptionDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<SubscriptionDatabaseOptions>>().Value;

            if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
                throw new InvalidOperationException("Subscription module connection string is missing.");

            options.UseNpgsql(dbOptions.ConnectionString);
        });

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<ISubscriptionModuleApi, SubscriptionModuleApi>();
        services.AddScoped<SubscriptionPlanSeeder>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SubscriptionDbContext>());

        services.Configure<SubscriptionIyzicoOptions>(
            configuration.GetSection(SubscriptionIyzicoOptions.SectionName));

        services.AddHttpClient<ISubscriptionPaymentGateway, IyzicoSubscriptionPaymentGateway>();

        return services;
    }
}
