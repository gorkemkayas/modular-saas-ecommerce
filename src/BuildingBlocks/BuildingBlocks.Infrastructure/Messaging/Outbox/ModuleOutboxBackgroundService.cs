using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class ModuleOutboxBackgroundService<TContext, TIdentity> : BackgroundService
    where TContext : DbContext
    where TIdentity : class
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ModuleOutboxOptions<TIdentity> _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ModuleOutboxBackgroundService<TContext, TIdentity>> _logger;

    public ModuleOutboxBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<ModuleOutboxOptions<TIdentity>> options,
        TimeProvider timeProvider,
        ILogger<ModuleOutboxBackgroundService<TContext, TIdentity>> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunProcessorAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An error occurred while processing the module Outbox.");
            }

            try
            {
                await Task.Delay(
                    _options.PollingInterval,
                    _timeProvider,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task RunProcessorAsync(
        CancellationToken cancellationToken)
    {
        await using var scope =
            _serviceScopeFactory.CreateAsyncScope();

        var processor = scope.ServiceProvider
            .GetRequiredService<ModuleOutboxProcessor<TContext, TIdentity>>();

        await processor.ProcessBatchAsync(
            cancellationToken);
    }
}
