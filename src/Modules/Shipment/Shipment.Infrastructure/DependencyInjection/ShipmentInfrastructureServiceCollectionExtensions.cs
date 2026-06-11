using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shipment.Application.Abstractions;
using Shipment.Application.Abstractions.Queries;
using Shipment.Application.Contracts;
using Shipment.Application.Integrations;
using Shipment.Contracts;
using Shipment.Domain.Repositories;
using Shipment.Infrastructure.Integrations.Order;
using Shipment.Infrastructure.Integrations.Notification;
using Shipment.Infrastructure.Options;
using Shipment.Infrastructure.Persistence;
using Shipment.Infrastructure.Persistence.Repositories;
using Shipment.Infrastructure.ReadServices;
using Shipment.Infrastructure.Services;

namespace Shipment.Infrastructure.DependencyInjection;

public static class ShipmentInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddShipmentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ShipmentDatabaseOptions>(
            configuration.GetSection(ShipmentDatabaseOptions.SectionName));

        services.AddDbContext<ShipmentDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<ShipmentDatabaseOptions>>().Value;

            if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
                throw new InvalidOperationException("Shipment module connection string is missing.");

            options.UseNpgsql(dbOptions.ConnectionString);
        });

        services.AddScoped<IOrderShipmentContextService, OrderShipmentContextService>();
        services.AddScoped<IOrderShipmentSyncService, OrderShipmentSyncService>();
        services.AddScoped<IShipmentNotificationService, ShipmentNotificationService>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IShippingCarrierRepository, ShippingCarrierRepository>();
        services.AddScoped<IShipmentReadService, ShipmentReadService>();
        services.AddScoped<IShippingCarrierReadService, ShippingCarrierReadService>();
        services.AddScoped<IShipmentModuleApi, ShipmentModuleApi>();
        services.AddScoped<IShipmentNumberGenerator, ShipmentNumberGenerator>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ShipmentDbContext>());

        return services;
    }
}
