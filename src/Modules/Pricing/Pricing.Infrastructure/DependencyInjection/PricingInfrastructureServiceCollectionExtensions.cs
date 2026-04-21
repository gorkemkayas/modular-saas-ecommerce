using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pricing.Application.Abstractions;
using Pricing.Application.Abstractions.Queries;
using Pricing.Application.Contracts;
using Pricing.Contracts;
using Pricing.Domain.Repositories;
using Pricing.Infrastructure.Options;
using Pricing.Infrastructure.Persistence;
using Pricing.Infrastructure.Persistence.Repositories;
using Pricing.Infrastructure.ReadServices;

namespace Pricing.Infrastructure.DependencyInjection;

public static class PricingInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPricingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PricingDatabaseOptions>(
            configuration.GetSection(PricingDatabaseOptions.SectionName));

        services.AddDbContext<PricingDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<PricingDatabaseOptions>>().Value;

            if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
                throw new InvalidOperationException("Pricing module connection string is missing.");

            options.UseNpgsql(dbOptions.ConnectionString);
        });

        services.AddScoped<IPriceListRepository, PriceListRepository>();
        services.AddScoped<IPriceListReadService, PriceListReadService>();
        services.AddScoped<IPriceResolutionReadService, PriceResolutionReadService>();
        services.AddScoped<IPriceCoverageReadService, PriceCoverageReadService>();
        services.AddScoped<IPricingModuleApi, PricingModuleApi>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PricingDbContext>());

        return services;
    }
}
