using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Order.Application.Abstractions;
using Order.Application.Abstractions.Queries;
using Order.Application.Contracts;
using Order.Application.Integrations;
using Order.Contracts;
using Order.Domain.Repositories;
using Order.Infrastructure.Integrations.Catalog;
using Order.Infrastructure.Integrations.Customer;
using Order.Infrastructure.Integrations.Inventory;
using Order.Infrastructure.Integrations.Notification;
using Order.Infrastructure.Integrations.Pricing;
using Order.Infrastructure.Integrations.Shipment;
using Order.Infrastructure.Options;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Repositories;
using Order.Infrastructure.ReadServices;
using Order.Infrastructure.Services;

namespace Order.Infrastructure.DependencyInjection;

public static class OrderInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddOrderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OrderDatabaseOptions>(
            configuration.GetSection(OrderDatabaseOptions.SectionName));

        services.AddDbContext<OrderDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<OrderDatabaseOptions>>().Value;

            if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
                throw new InvalidOperationException("Order module connection string is missing.");

            options.UseNpgsql(dbOptions.ConnectionString);
        });

        services.AddScoped<IOrderCustomerContextService, OrderCustomerContextService>();
        services.AddScoped<IOrderCatalogProductService, OrderCatalogProductService>();
        services.AddScoped<IOrderPricingService, OrderPricingService>();
        services.AddScoped<IOrderInventoryService, OrderInventoryService>();
        services.AddScoped<IOrderShippingCarrierService, OrderShippingCarrierService>();
        services.AddScoped<IOrderNotificationService, OrderNotificationService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderReadService, OrderReadService>();
        services.AddScoped<IOrderModuleApi, OrderModuleApi>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrderDbContext>());
        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();

        return services;
    }
}
