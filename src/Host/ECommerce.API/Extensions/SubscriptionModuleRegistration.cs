using ECommerce.API.ExceptionHandlers;
using Subscription.Infrastructure.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class SubscriptionModuleRegistration
{
    public static IServiceCollection AddSubscriptionModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<SubscriptionExceptionHandler>();

        return services.AddSubscriptionInfrastructure(configuration);
    }
}
