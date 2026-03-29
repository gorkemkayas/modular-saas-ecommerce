using Catalog.Infrastructure.DependencyInjection;
using ECommerce.API.ExceptionHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.API.Extensions
{
    public static class CatalogModuleRegistration
    {
        public static IServiceCollection AddCatalogModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddExceptionHandler<CatalogExceptionHandler>();
            return services.AddCatalogInfrastructure(configuration);
        }
    }
}
