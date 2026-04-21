using ECommerce.API.ExceptionHandlers;
using ECommerce.API.Integrations.Pricing;
using Pricing.Application.Integrations;
using Pricing.Infrastructure.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class PricingModuleRegistration
{
    public static IServiceCollection AddPricingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<PricingExceptionHandler>();
        services.AddScoped<ICatalogSellableItemValidator, CatalogSellableItemValidator>();

        return services.AddPricingInfrastructure(configuration);
    }
}
