using Inventory.Application.Abstractions;
using Inventory.Application.Abstractions.Queries;
using Inventory.Application.Contracts;
using Inventory.Application.Integrations;
using Inventory.Contracts;
using Inventory.Domain.Repositories;
using Inventory.Infrastructure.Integrations.Catalog;
using Inventory.Infrastructure.Options;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.Repositories;
using Inventory.Infrastructure.ReadServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Inventory.Infrastructure.DependencyInjection;

public static class InventoryInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<InventoryDatabaseOptions>(
            configuration.GetSection(InventoryDatabaseOptions.SectionName));

        services.AddDbContext<InventoryDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<InventoryDatabaseOptions>>().Value;

            if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
                throw new InvalidOperationException("Inventory module connection string is missing.");

            options.UseNpgsql(dbOptions.ConnectionString);
        });

        services.AddScoped<IInventoryCatalogService, InventoryCatalogService>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IInventoryReadService, InventoryReadService>();
        services.AddScoped<IInventoryModuleApi, InventoryModuleApi>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<InventoryDbContext>());

        return services;
    }
}
