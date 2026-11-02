using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox;

public sealed class ModuleInboxBackgroundService<TContext, TIdentity>(
    ModuleInboxProcessor<TContext, TIdentity> processor,
    IOptions<ModuleInboxOptions<TIdentity>> options,
    TimeProvider timeProvider,
    ILogger<ModuleInboxBackgroundService<TContext, TIdentity>> logger) : BackgroundService
    where TContext : DbContext
    where TIdentity : class
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled) return;
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await processor.ProcessBatchAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { logger.LogError(exception, "Module Inbox batch failed."); }
            try { await Task.Delay(options.Value.PollingInterval, timeProvider, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
