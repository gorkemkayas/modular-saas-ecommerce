using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notification.Application.Abstractions;
using Notification.Application.Abstractions.Queries;
using Notification.Application.Contracts;
using Notification.Application.Notifications.Services;
using Notification.Contracts;
using Notification.Domain.Repositories;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Repositories;
using Notification.Infrastructure.ReadServices;
using Notification.Infrastructure.Seeding;
using Notification.Infrastructure.Services;

namespace Notification.Infrastructure.DependencyInjection;

public static class NotificationInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<NotificationDatabaseOptions>(
            configuration.GetSection(NotificationDatabaseOptions.SectionName));
        services.Configure<NotificationEmailOptions>(
            configuration.GetSection(NotificationEmailOptions.SectionName));

        services.AddDbContext<NotificationDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<NotificationDatabaseOptions>>().Value;

            if (string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
                throw new InvalidOperationException("Notification module connection string is missing.");

            options.UseNpgsql(dbOptions.ConnectionString);
        });

        services.AddHttpClient<ResendEmailGateway>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NotificationEmailOptions>>().Value;
            var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
                ? "https://api.resend.com"
                : options.BaseUrl.Trim();

            client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/", UriKind.Absolute);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("modular-saas-ecommerce/1.0");
        });

        services.AddScoped<IContactFeedbackRepository, ContactFeedbackRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<INotificationDispatchRepository, NotificationDispatchRepository>();
        services.AddScoped<INotificationReadService, NotificationReadService>();
        services.AddScoped<ITemplateRenderer, SimpleTemplateRenderer>();
        services.AddScoped<MockEmailGateway>();
        services.AddScoped<ResendWebhookVerifier>();
        services.AddScoped<INotificationSender, NotificationSender>();
        services.AddScoped<INotificationModuleApi, NotificationModuleApi>();
        services.AddScoped<NotificationTemplateSeeder>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NotificationDbContext>());
        services.AddScoped<IResendWebhookVerifier>(sp => sp.GetRequiredService<ResendWebhookVerifier>());
        services.AddScoped<IEmailGateway>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<NotificationEmailOptions>>().Value;

            return options.Provider.Equals("Resend", StringComparison.OrdinalIgnoreCase)
                ? sp.GetRequiredService<ResendEmailGateway>()
                : sp.GetRequiredService<MockEmailGateway>();
        });

        return services;
    }
}
