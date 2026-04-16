using Customer.Infrastructure.DependencyInjection;
using ECommerce.API.ExceptionHandlers;

namespace ECommerce.API.Extensions;

public static class CustomerModuleRegistration
{
    public static IServiceCollection AddCustomerModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<CustomerExceptionHandler>();
        return services.AddCustomerInfrastructure(configuration);
    }
}
