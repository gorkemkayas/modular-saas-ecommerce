using Customer.Application.Abstractions;
using Customer.Application.Abstractions.Queries;
using Customer.Application.Contracts;
using Customer.Contracts;
using Customer.Domain.Repositories;
using Customer.Infrastructure.Options;
using Customer.Infrastructure.Persistence;
using Customer.Infrastructure.Persistence.Repositories;
using Customer.Infrastructure.ReadServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Customer.Infrastructure.DependencyInjection;

public static class CustomerInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CustomerDatabaseOptions>(
            configuration.GetSection(CustomerDatabaseOptions.SectionName));

        services.AddDbContext<CustomerDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<CustomerDatabaseOptions>>().Value;

            if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
                throw new InvalidOperationException("Customer module connection string is missing.");

            options.UseNpgsql(dbOptions.ConnectionString);
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerReadService, CustomerReadService>();
        services.AddScoped<ICustomerModuleApi, CustomerModuleApi>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CustomerDbContext>());

        return services;
    }
}
