using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Infrastructure.Options;

namespace Notification.Infrastructure.Messaging.Dispatch;

public sealed class NotificationDispatchBackgroundService : BackgroundService
{
    private readonly NotificationDispatchProcessor _processor;
    private readonly NotificationDispatchOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<NotificationDispatchBackgroundService> _logger;

    public NotificationDispatchBackgroundService(
        NotificationDispatchProcessor processor,
        IOptions<NotificationDispatchOptions> options,
        TimeProvider timeProvider,
        ILogger<NotificationDispatchBackgroundService> logger)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);
        _processor = processor;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Notification dispatch processor is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _processor.ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Notification dispatch batch failed.");
            }

            try
            {
                await Task.Delay(_options.PollingInterval, _timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
