using System.Net.Mail;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Notifications.Services;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Exceptions;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Dispatch;

namespace Notification.Infrastructure.Messaging.Dispatch;

public sealed class NotificationDispatchProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NotificationDispatchOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<NotificationDispatchProcessor> _logger;

    public NotificationDispatchProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<NotificationDispatchOptions> options,
        TimeProvider timeProvider,
        ILogger<NotificationDispatchProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ClaimedNotificationDispatch> deliveries;
        await using (AsyncServiceScope scope = _scopeFactory.CreateAsyncScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<INotificationDispatchStore>();
            DateTime now = UtcNow();
            deliveries = await store.ClaimAsync(
                Guid.NewGuid(), now, now.Add(_options.ClaimDuration),
                _options.BatchSize, cancellationToken);
        }

        foreach (ClaimedNotificationDispatch delivery in deliveries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await ProcessDeliveryAsync(delivery, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                // Persistence failures may follow a successful provider call. Leave the
                // lease in place; recovery reuses the frozen payload and idempotency key.
                _logger.LogError(exception,
                    "Dispatch {DispatchId} could not be finalized. It becomes claimable after lease expiry.",
                    delivery.Id);
            }
        }
    }

    private async Task ProcessDeliveryAsync(
        ClaimedNotificationDispatch delivery,
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<INotificationDispatchStore>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        if (!await RenewLeaseAsync(store, delivery, cancellationToken))
            return;

        NotificationDispatch dispatch = await dbContext.NotificationDispatches
            .AsNoTracking()
            .SingleAsync(x => x.Id == delivery.Id, cancellationToken);

        if (dispatch.Status != NotificationStatus.Pending)
        {
            if (dispatch.Status is NotificationStatus.Sent or NotificationStatus.Suppressed)
                await store.MarkCompletedAsync(delivery.Id, delivery.LockId, cancellationToken);
            else
                await store.MarkFailedAsync(delivery.Id, delivery.LockId,
                    "Dispatch is already in a terminal state.", cancellationToken);
            return;
        }

        var gateway = scope.ServiceProvider.GetRequiredService<IEmailGateway>();
        string providerName = delivery.ProviderName ?? gateway.ProviderName;
        DateTime? preparedAtUtc = delivery.PreparedAtUtc;
        EmailSendResult result;

        if (delivery.AttemptCount > _options.MaximumAttempts)
        {
            result = Failure("attempts_exhausted", "Delivery lease recovery exhausted the attempt budget.");
        }
        else if (preparedAtUtc.HasValue &&
                 UtcNow() >= preparedAtUtc.Value.Add(_options.MaximumDeliveryAge))
        {
            result = Failure("idempotency_window_expired", "The safe automatic delivery retry window expired.");
        }
        else
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_options.SendTimeout);

            try
            {
                if (!string.Equals(providerName, gateway.ProviderName, StringComparison.Ordinal))
                    throw new InvalidDataException("The configured email provider changed after delivery preparation.");

                EmailSendRequest request;
                if (delivery.Payload is null)
                {
                    request = await PrepareRequestAsync(scope.ServiceProvider, dispatch, timeout.Token);
                    preparedAtUtc = UtcNow();
                    bool prepared = await store.PrepareAsync(
                        delivery.Id, delivery.LockId, JsonSerializer.Serialize(request, JsonOptions),
                        providerName, preparedAtUtc.Value, timeout.Token);
                    if (!prepared)
                        return;
                }
                else
                {
                    request = JsonSerializer.Deserialize<EmailSendRequest>(delivery.Payload, JsonOptions)
                        ?? throw new InvalidDataException("The persisted email request is invalid.");
                }

                // A batch can outlive its original lease. Never send on an expired or stolen claim.
                if (!await RenewLeaseAsync(store, delivery, timeout.Token))
                    return;

                result = await gateway.SendAsync(request, timeout.Token);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                bool isPermanent = exception is InvalidDataException
                    or JsonException or NotificationDomainException or FormatException;
                result = new EmailSendResult(false, null, null,
                    isPermanent ? "invalid_delivery" : "delivery_error",
                    Truncate(exception.Message, 1000), IsTransientFailure: !isPermanent);
                _logger.LogWarning(exception,
                    "Dispatch {DispatchId} failed during preparation or sending on attempt {AttemptCount}.",
                    delivery.Id, delivery.AttemptCount);
            }
        }

        await PersistOutcomeAsync(delivery, providerName, preparedAtUtc, result, cancellationToken);
    }

    private async Task<EmailSendRequest> PrepareRequestAsync(
        IServiceProvider services, NotificationDispatch dispatch, CancellationToken cancellationToken)
    {
        if (dispatch.Channel != NotificationChannel.Email ||
            !MailAddress.TryCreate(dispatch.RecipientAddress, out _) ||
            string.IsNullOrWhiteSpace(dispatch.Subject) || string.IsNullOrWhiteSpace(dispatch.Body))
            throw new InvalidDataException("Email delivery requires a valid recipient and rendered content.");

        var brandingProvider = services.GetRequiredService<IStoreBrandingProvider>();
        var composer = services.GetRequiredService<IEmailLayoutComposer>();
        var emailOptions = services.GetRequiredService<IOptions<NotificationEmailOptions>>().Value;
        var branding = await brandingProvider.GetAsync(dispatch.StoreId, cancellationToken);
        string html = composer.ComposeHtml(new EmailComposition(
            branding?.Name ?? string.Empty, dispatch.Subject, dispatch.Body,
            null, Array.Empty<EmailDetailRow>(), Array.Empty<EmailLineItem>(),
            Array.Empty<EmailDetailRow>(), "Order Update"));

        string from = string.IsNullOrWhiteSpace(emailOptions.FromName)
            ? emailOptions.FromEmail.Trim()
            : $"{emailOptions.FromName.Trim()} <{emailOptions.FromEmail.Trim()}>";

        return new EmailSendRequest(
            dispatch.StoreId, dispatch.RecipientAddress!, dispatch.RecipientName,
            dispatch.Subject, dispatch.Body, html,
            $"notification-dispatch/{dispatch.Id:D}",
            new EmailSender(from, emailOptions.ReplyTo));
    }

    private async Task PersistOutcomeAsync(
        ClaimedNotificationDispatch delivery, string providerName, DateTime? preparedAtUtc,
        EmailSendResult result, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<INotificationDispatchStore>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        DateTime now = UtcNow();
        DateTime nextAttempt = now.Add(CalculateRetryDelay(delivery.AttemptCount));
        if (result.RetryAfter is { } retryAfter && retryAfter > TimeSpan.Zero)
        {
            TimeSpan boundedDelay = retryAfter > _options.MaximumDeliveryAge
                ? _options.MaximumDeliveryAge : retryAfter;
            DateTime providerNextAttempt = now.Add(boundedDelay);
            if (providerNextAttempt > nextAttempt)
                nextAttempt = providerNextAttempt;
        }

        bool retry = !result.IsSuccess && result.IsTransientFailure &&
            delivery.AttemptCount < _options.MaximumAttempts &&
            (!preparedAtUtc.HasValue || nextAttempt < preparedAtUtc.Value.Add(_options.MaximumDeliveryAge));

        string error = $"{result.FailureCode}: {result.FailureMessage}";
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        bool ownsDelivery;
        if (result.IsSuccess)
            ownsDelivery = await store.MarkCompletedAsync(delivery.Id, delivery.LockId, cancellationToken);
        else if (retry)
            ownsDelivery = await store.ScheduleRetryAsync(
                delivery.Id, delivery.LockId, nextAttempt, error, cancellationToken);
        else
            ownsDelivery = await store.MarkFailedAsync(delivery.Id, delivery.LockId, error, cancellationToken);

        if (!ownsDelivery)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning("Dispatch {DispatchId} outcome was discarded because ownership was lost.", delivery.Id);
            return;
        }

        NotificationDispatch dispatch = await dbContext.NotificationDispatches
            .Include(x => x.Attempts)
            .SingleAsync(x => x.Id == delivery.Id, cancellationToken);

        if (dispatch.Status == NotificationStatus.Pending)
        {
            if (result.IsSuccess)
                dispatch.MarkSent(providerName, Truncate(result.ProviderRequestReference, 200),
                    Truncate(result.ProviderMessageId, 200));
            else if (retry)
                dispatch.RecordFailedAttempt(providerName, Truncate(result.ProviderRequestReference, 200),
                    Truncate(result.FailureCode, 100), Truncate(result.FailureMessage, 1000));
            else
                dispatch.MarkFailed(providerName, Truncate(result.ProviderRequestReference, 200),
                    Truncate(result.FailureCode, 100), Truncate(result.FailureMessage, 1000));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _logger.LogInformation(
            "Dispatch {DispatchId} delivery outcome persisted. Success: {Success}, Retry: {Retry}, Attempt: {Attempt}.",
            delivery.Id, result.IsSuccess, retry, delivery.AttemptCount);
    }

    private async Task<bool> RenewLeaseAsync(
        INotificationDispatchStore store, ClaimedNotificationDispatch delivery,
        CancellationToken cancellationToken)
    {
        DateTime now = UtcNow();
        bool renewed = await store.RenewLeaseAsync(
            delivery.Id, delivery.LockId, now, now.Add(_options.ClaimDuration), cancellationToken);
        if (!renewed)
            _logger.LogWarning("Dispatch {DispatchId} is no longer owned by this worker; sending was skipped.", delivery.Id);
        return renewed;
    }

    private TimeSpan CalculateRetryDelay(int attempt)
    {
        double capped = Math.Min(_options.InitialRetryDelay.TotalMilliseconds *
            Math.Pow(2, Math.Min(30, Math.Max(0, attempt - 1))), _options.MaximumRetryDelay.TotalMilliseconds);
        double jitter = 1 + ((Random.Shared.NextDouble() * 2) - 1) * _options.RetryJitterFactor;
        return TimeSpan.FromMilliseconds(Math.Clamp(capped * jitter, 0, _options.MaximumRetryDelay.TotalMilliseconds));
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
    private static EmailSendResult Failure(string code, string message) => new(false, null, null, code, message);
    private static string? Truncate(string? value, int length) => value?.Length > length ? value[..length] : value;
}
