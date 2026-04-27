using ECommerce.API.ExceptionHandlers;
using Pricing.Infrastructure.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class PricingModuleRegistration
{
    public static IServiceCollection AddPricingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<PricingExceptionHandler>();

        return services.AddPricingInfrastructure(configuration);
    }
}
