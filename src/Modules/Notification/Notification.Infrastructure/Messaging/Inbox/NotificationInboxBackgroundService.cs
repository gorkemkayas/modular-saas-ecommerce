using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Infrastructure.Options;

namespace Notification.Infrastructure.Messaging.Inbox;

public sealed class NotificationInboxBackgroundService
    : BackgroundService
{
    private readonly NotificationInboxProcessor _processor;
    private readonly NotificationInboxOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<NotificationInboxBackgroundService> _logger;

    public NotificationInboxBackgroundService(
        NotificationInboxProcessor processor,
        IOptions<NotificationInboxOptions> options,
        TimeProvider timeProvider,
        ILogger<NotificationInboxBackgroundService> logger)
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

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Notification Inbox processor is disabled.");

            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _processor.ProcessBatchAsync(
                    stoppingToken);
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
                    "An error occurred while processing " +
                    "the Notification Inbox.");
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
}