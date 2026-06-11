using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Subscription.Infrastructure.Seeding;

namespace Subscription.Infrastructure.DependencyInjection;

public static class SubscriptionApplicationBuilderExtensions
{
    public static async Task SeedSubscriptionDefaultsAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("SubscriptionPlanSeeding");

        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<SubscriptionPlanSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Subscription default plan seeding was skipped because the subscription store was not ready.");
        }
    }
}
