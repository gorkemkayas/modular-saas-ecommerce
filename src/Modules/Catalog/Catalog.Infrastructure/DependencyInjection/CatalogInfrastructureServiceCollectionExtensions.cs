using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Queries;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Options;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Catalog.Infrastructure.ReadServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Catalog.Infrastructure.DependencyInjection
{
    public static class CatalogInfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddCatalogInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<CatalogDatabaseOptions>(
                configuration.GetSection(CatalogDatabaseOptions.SectionName));

            services.AddDbContext<CatalogDbContext>((sp, options) =>
            {
                var dbOptions = sp.GetRequiredService<IOptions<CatalogDatabaseOptions>>().Value;

                if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
                    throw new InvalidOperationException("Catalog module connection string is missing.");

                options.UseNpgsql(dbOptions.ConnectionString);
            });

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<IAttributeDefinitionRepository, AttributeDefinitionRepository>();

            services.AddScoped<IProductReadService, ProductReadService>();
            services.AddScoped<ICategoryReadService, CategoryReadService>();
            services.AddScoped<IBrandReadService, BrandReadService>();
            services.AddScoped<IAttributeDefinitionReadService, AttributeDefinitionReadService>();

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());

            return services;
        }
    }
}
