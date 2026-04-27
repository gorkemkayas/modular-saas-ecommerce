using ECommerce.API.ExceptionHandlers;
using ECommerce.API.Integrations.Order;
using Order.Application.Integrations;
using Order.Infrastructure.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class OrderModuleRegistration
{
    public static IServiceCollection AddOrderModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<OrderExceptionHandler>();
        services.AddScoped<IOrderCustomerContextService, OrderCustomerContextService>();
        services.AddScoped<IOrderCatalogProductService, OrderCatalogProductService>();
        services.AddScoped<IOrderPricingService, OrderPricingService>();
        services.AddScoped<IOrderInventoryService, NoOpOrderInventoryService>();

        return services.AddOrderInfrastructure(configuration);
    }
}
