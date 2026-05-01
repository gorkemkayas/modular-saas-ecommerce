using ECommerce.API.ExceptionHandlers;
using Shipment.Infrastructure.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class ShipmentModuleRegistration
{
    public static IServiceCollection AddShipmentModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<ShipmentExceptionHandler>();

        return services.AddShipmentInfrastructure(configuration);
    }
}
