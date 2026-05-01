using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Payment.Application.Abstractions;
using Payment.Application.Abstractions.Queries;
using Payment.Application.Contracts;
using Payment.Application.Integrations;
using Payment.Contracts;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;
using Payment.Infrastructure.Gateways;
using Payment.Infrastructure.Integrations.Inventory;
using Payment.Infrastructure.Integrations.Order;
using Payment.Infrastructure.Integrations.Shipment;
using Payment.Infrastructure.Options;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Repositories;
using Payment.Infrastructure.ReadServices;

namespace Payment.Infrastructure.DependencyInjection;

public static class PaymentInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PaymentDatabaseOptions>(
            configuration.GetSection(PaymentDatabaseOptions.SectionName));
        services.Configure<PaymentGatewayOptions>(
            configuration.GetSection(PaymentGatewayOptions.SectionName));
        services.Configure<IyzicoOptions>(
            configuration.GetSection(IyzicoOptions.SectionName));

        services.AddDbContext<PaymentDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<PaymentDatabaseOptions>>().Value;

            if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
                throw new InvalidOperationException("Payment module connection string is missing.");

            options.UseNpgsql(dbOptions.ConnectionString);
        });

        services.AddHttpClient<IyzicoPaymentGateway>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<IyzicoOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
        });

        services.AddScoped<IOrderPaymentContextService, OrderPaymentContextService>();
        services.AddScoped<IOrderPaymentSyncService, OrderPaymentSyncService>();
        services.AddScoped<IInventoryPaymentService, PaymentInventoryService>();
        services.AddScoped<IShipmentPaymentService, PaymentShipmentService>();
        services.AddScoped<IPaymentWebhookParser, PaymentWebhookParser>();
        services.AddScoped<MockPaymentGateway>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentReadService, PaymentReadService>();
        services.AddScoped<IPaymentModuleApi, PaymentModuleApi>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentDbContext>());
        services.AddScoped<IPaymentGateway>(sp =>
        {
            var gatewayOptions = sp.GetRequiredService<IOptions<PaymentGatewayOptions>>().Value;

            if (Enum.TryParse<PaymentProvider>(gatewayOptions.Provider, ignoreCase: true, out var provider)
                && provider == PaymentProvider.Iyzico)
            {
                return sp.GetRequiredService<IyzicoPaymentGateway>();
            }

            return sp.GetRequiredService<MockPaymentGateway>();
        });

        return services;
    }
}
