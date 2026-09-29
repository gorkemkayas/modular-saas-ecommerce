using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.Infrastructure.Options;

namespace Order.Infrastructure.Outbox;

public sealed class OrderOutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly OrderOutboxOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OrderOutboxBackgroundService> _logger;

    public OrderOutboxBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<OrderOutboxOptions> options,
        TimeProvider timeProvider,
        ILogger<OrderOutboxBackgroundService> logger)
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
                    "An error occurred while processing the Order Outbox.");
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
            .GetRequiredService<OrderOutboxProcessor>();

        await processor.ProcessBatchAsync(
            cancellationToken);
    }
}
