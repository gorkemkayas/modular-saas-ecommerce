using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Store.Application.Abstractions;
using Store.Domain.Stores;
using Store.Infrastructure.Options;
using Store.Infrastructure.Persistance;
using Store.Infrastructure.Persistance.Repositories;

namespace Store.Infrastructure.DependencyInjection
{
    public static class StoreModuleRegistration
    {
        public static IServiceCollection AddStoreModule(
       this IServiceCollection services,
       IConfiguration configuration)
        {
            services.Configure<StoreDatabaseOptions>(
                configuration.GetSection(StoreDatabaseOptions.SectionName));

            services.AddDbContext<StoreDbContext>((sp, options) =>
            {
                var dbOptions = sp.GetRequiredService<IOptions<StoreDatabaseOptions>>().Value;

                if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
                    throw new InvalidOperationException("Store module connection string is missing.");

                options.UseNpgsql(dbOptions.ConnectionString);
            });

            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<StoreDbContext>());

            return services;
        }
    }
}
