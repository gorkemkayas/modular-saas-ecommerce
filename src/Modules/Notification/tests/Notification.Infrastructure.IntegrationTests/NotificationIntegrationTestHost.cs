using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Notifications.Services;
using Notification.Domain.Repositories;
using Notification.Infrastructure.Messaging.Inbox;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Inbox;
using Notification.Infrastructure.Persistence.Repositories;
using Notification.Infrastructure.Services;

namespace Notification.Infrastructure.IntegrationTests;

internal static class NotificationIntegrationTestHost
{
    public static ServiceProvider CreateServiceProvider(
        Action<IServiceCollection>? configureServices = null,
        NotificationInboxOptions? inboxOptions = null,
        TimeProvider? timeProvider = null)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<NotificationDbContext>(options =>
        {
            options.UseNpgsql(
                PostgreSqlTestContainer.ConnectionString);
        });

        services.AddScoped<
            IInboxMessageWriter,
            InboxMessageWriter>();

        services.AddScoped<
            INotificationInboxStore,
            NotificationInboxStore>();

        services.AddScoped<
            INotificationDispatchRepository,
            NotificationDispatchRepository>();

        services.AddScoped<
            INotificationTemplateRepository,
            NotificationTemplateRepository>();

        services.AddScoped<
            ITemplateRenderer,
            SimpleTemplateRenderer>();

        services.AddScoped<
            INotificationInboxMessageHandler,
            OrderCancelledInboxMessageHandler>();

        services.AddScoped<
            NotificationInboxMessageHandlerResolver>();

        services.AddSingleton(
            Microsoft.Extensions.Options.Options.Create(
                inboxOptions ?? new NotificationInboxOptions()));

        services.AddSingleton(
            timeProvider ?? TimeProvider.System);

        services.AddSingleton<NotificationInboxProcessor>();

        configureServices?.Invoke(services);

        return services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
            });
    }
}
