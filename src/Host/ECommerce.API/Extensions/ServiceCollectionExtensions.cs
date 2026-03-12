using Microsoft.Extensions.DependencyInjection;
using ECommerce.API.ExceptionHandlers;

namespace ECommerce.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStoreModule(this IServiceCollection services)
        {
            // Exception Handlers
            services.AddExceptionHandler<StoreExceptionHandler>();

            // Diğer Store module servisleri...

            return services;
        }
    }
}
