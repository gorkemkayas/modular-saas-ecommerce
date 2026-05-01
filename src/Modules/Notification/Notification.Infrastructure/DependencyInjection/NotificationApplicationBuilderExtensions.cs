using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notification.Infrastructure.Seeding;

namespace Notification.Infrastructure.DependencyInjection;

public static class NotificationApplicationBuilderExtensions
{
    public static async Task SeedNotificationDefaultsAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("NotificationTemplateSeeding");

        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<NotificationTemplateSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Notification default template seeding was skipped because the notification store was not ready.");
        }
    }
}
