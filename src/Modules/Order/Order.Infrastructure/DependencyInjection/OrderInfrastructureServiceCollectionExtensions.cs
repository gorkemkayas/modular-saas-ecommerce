using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Order.Application.Abstractions;
using Order.Application.Abstractions.Queries;
using Order.Application.Contracts;
using Order.Application.Events;
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
using Order.Infrastructure.Persistence.Outbox;
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
        services.AddOptions<OrderOutboxOptions>()
            .Bind(
                configuration.GetSection(
                    OrderOutboxOptions.SectionName))
            .Validate(
                options =>
                    options.BatchSize is > 0 and <= 1000,
                "Order Outbox batch size must be between 1 and 1000.")
            .Validate(
                options =>
                    options.PollingInterval > TimeSpan.Zero,
                "Order Outbox polling interval must be greater than zero.")
            .Validate(
                options =>
                    options.ClaimDuration > TimeSpan.Zero,
                "Order Outbox claim duration must be greater than zero.")
            .Validate(
                options =>
                    options.MaximumAttempts > 0,
                "Order Outbox maximum attempts must be greater than zero.")
            .Validate(
                options =>
                    options.InitialRetryDelay > TimeSpan.Zero,
                "Order Outbox initial retry delay must be greater than zero.")
            .Validate(
                options =>
                    options.MaximumRetryDelay >=
                    options.InitialRetryDelay,
                "Order Outbox maximum retry delay must be greater than " +
                "or equal to the initial retry delay.")
            .ValidateOnStart();

        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
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
        services.AddScoped<IOrderDomainEventDispatcher, OrderDomainEventDispatcher>();
        services.AddScoped<IOrderOutboxWriter, OrderOutboxWriter>();
        services.AddScoped<IOrderOutboxStore, OrderOutboxStore>();
        return services;
    }
}
