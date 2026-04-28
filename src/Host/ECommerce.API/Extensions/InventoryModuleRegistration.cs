using ECommerce.API.ExceptionHandlers;
using Inventory.Infrastructure.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class InventoryModuleRegistration
{
    public static IServiceCollection AddInventoryModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<InventoryExceptionHandler>();

        return services.AddInventoryInfrastructure(configuration);
    }
}
