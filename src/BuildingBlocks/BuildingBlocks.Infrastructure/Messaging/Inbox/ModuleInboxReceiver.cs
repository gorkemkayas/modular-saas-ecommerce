using BuildingBlocks.Messaging.Abstractions.Publishing;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox;

public sealed class ModuleInboxReceiver<TContext, TIdentity>(
    IModuleInboxStore<TContext> store, TimeProvider timeProvider)
    : IIntegrationMessageReceiver<TIdentity>
    where TContext : DbContext
    where TIdentity : class
{
    public async Task ReceiveAsync(IntegrationMessage message, CancellationToken cancellationToken = default)
    {
        await store.WriteAsync(message, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }
}
