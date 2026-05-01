using ECommerce.API.ExceptionHandlers;
using Notification.Infrastructure.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class NotificationModuleRegistration
{
    public static IServiceCollection AddNotificationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<NotificationExceptionHandler>();

        return services.AddNotificationInfrastructure(configuration);
    }
}
