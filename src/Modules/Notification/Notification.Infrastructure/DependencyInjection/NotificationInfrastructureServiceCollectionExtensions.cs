using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Notification.Application.Abstractions;
using Notification.Application.Abstractions.Queries;
using Notification.Application.Contracts;
using Notification.Application.Notifications.Services;
using Notification.Contracts;
using Notification.Domain.Repositories;
using Notification.Infrastructure.Messaging.Inbox;
using Notification.Infrastructure.Messaging.Dispatch;
using Notification.Infrastructure.Messaging.RabbitMq;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Inbox;
using Notification.Infrastructure.Persistence.Dispatch;
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
        services.Configure<NotificationBrandingOptions>(
            configuration.GetSection(NotificationBrandingOptions.SectionName));
        services.AddOptions<NotificationIntegrationEventConsumerOptions>()
            .Configure(options =>
            {
                IConfigurationSection section =
                    configuration.GetSection(
                        NotificationIntegrationEventConsumerOptions.SectionName);

                section.Bind(options);

                TimeSpan[]? configuredDelays = section
                    .GetSection("Retry:Delays")
                    .Get<TimeSpan[]>();

                if (configuredDelays is not null)
                {
                    options.Retry.Delays = configuredDelays;
                }
            })
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.QueueName),
                "Notification integration-event queue name is required.")
            .Validate(
                options =>
                    options.QueueName.Length <= 255,
                "Notification integration-event queue name cannot exceed 255 characters.")
            .Validate(
                options =>
                    options.PrefetchCount is > 0 and <= 1000,
                "Notification consumer prefetch count must be between 1 and 1000.")
            .Validate(
                options =>
                    options.Retry?.Delays is
                        { Length: > 0 and <= 10 },
                "Notification consumer must define between 1 and 10 retry delays.")
            .Validate(
                options =>
                    options.Retry?.Delays.All(
                        delay => delay > TimeSpan.Zero) == true,
                "Notification consumer retry delays must be greater than zero.")
            .Validate(
                options =>
                    options.Retry?.Delays is { } delays &&
                    delays
                        .Select((delay, index) =>
                            index == 0 ||
                            delay > delays[index - 1])
                        .All(result => result),
                "Notification consumer retry delays must be strictly increasing.")
            .ValidateOnStart();

        services.AddOptions<NotificationInboxOptions>()
            .Bind(
                configuration.GetSection(
                    NotificationInboxOptions.SectionName))
            .Validate(
                options =>
                    options.BatchSize is > 0 and <= 1000,
                "Notification Inbox batch size must be between 1 and 1000.")
            .Validate(
                options =>
                    options.PollingInterval > TimeSpan.Zero,
                "Notification Inbox polling interval must be greater than zero.")
            .Validate(
                options =>
                    options.ClaimDuration > TimeSpan.Zero,
                "Notification Inbox claim duration must be greater than zero.")
            .Validate(
                options =>
                    options.MaximumAttempts > 0,
                "Notification Inbox maximum attempts must be greater than zero.")
            .Validate(
                options =>
                    options.InitialRetryDelay > TimeSpan.Zero,
                "Notification Inbox initial retry delay must be greater than zero.")
            .Validate(
                options =>
                    options.MaximumRetryDelay >=
                    options.InitialRetryDelay,
                "Notification Inbox maximum retry delay must be greater than " +
                "or equal to the initial retry delay.")
            .Validate(
                options =>
                    options.RetryJitterFactor is >= 0 and <= 1,
                "Notification Inbox retry jitter factor must be between 0 and 1.")
            .ValidateOnStart();

        services.AddOptions<NotificationDispatchOptions>()
            .Bind(configuration.GetSection(NotificationDispatchOptions.SectionName))
            .Validate(options => options.BatchSize is > 0 and <= 1000,
                "Notification dispatch batch size must be between 1 and 1000.")
            .Validate(options => options.PollingInterval > TimeSpan.Zero,
                "Notification dispatch polling interval must be positive.")
            .Validate(options => options.SendTimeout > TimeSpan.Zero &&
                options.ClaimDuration > options.SendTimeout,
                "Notification dispatch claim duration must exceed its positive send timeout.")
            .Validate(options => options.MaximumAttempts is > 0 and <= 1000,
                "Notification dispatch maximum attempts must be between 1 and 1000.")
            .Validate(options => options.InitialRetryDelay > TimeSpan.Zero &&
                options.MaximumRetryDelay >= options.InitialRetryDelay,
                "Notification dispatch retry delays must be positive and ordered.")
            .Validate(options => options.RetryJitterFactor is >= 0 and <= 1,
                "Notification dispatch jitter must be between 0 and 1.")
            .Validate(options => options.MaximumDeliveryAge > options.MaximumRetryDelay &&
                options.MaximumDeliveryAge <= TimeSpan.FromHours(23),
                "Notification dispatch delivery age must exceed retry delay and remain within 23 hours.")
            .ValidateOnStart();

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
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<IInboxMessageWriter, InboxMessageWriter>();
        services.AddScoped<INotificationInboxStore, NotificationInboxStore>();
        services.AddScoped<INotificationInboxMessageHandler, OrderCancelledInboxMessageHandler>();
        services.AddScoped<NotificationInboxMessageHandlerResolver>();
        services.AddSingleton<NotificationInboxProcessor>();
        services.AddScoped<INotificationDispatchStore, NotificationDispatchStore>();
        services.AddSingleton<NotificationDispatchProcessor>();
        services.AddSingleton<NotificationIntegrationEventFailureRouter>();

        services.AddHostedService<NotificationInboxBackgroundService>();
        services.AddHostedService<NotificationDispatchBackgroundService>();
        services.AddHostedService<NotificationIntegrationEventConsumer>();

        services.AddScoped<IContactFeedbackRepository, ContactFeedbackRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<INotificationDispatchRepository, NotificationDispatchRepository>();
        services.AddScoped<INotificationReadService, NotificationReadService>();
        services.AddScoped<ITemplateRenderer, SimpleTemplateRenderer>();
        services.AddScoped<IEmailLayoutComposer, BrandedEmailComposer>();
        services.AddScoped<IStoreBrandingProvider, StoreBrandingProvider>();
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
