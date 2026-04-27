using ECommerce.API.ExceptionHandlers;
using Order.Infrastructure.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class OrderModuleRegistration
{
    public static IServiceCollection AddOrderModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<OrderExceptionHandler>();

        return services.AddOrderInfrastructure(configuration);
    }
}
