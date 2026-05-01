using Notification.Domain.Common;
using Notification.Domain.Enums;
using Notification.Domain.Exceptions;

namespace Notification.Domain.Entities;

public sealed class NotificationDispatch : IAggregateRoot
{
    private readonly List<NotificationAttempt> _attempts = new();

    private NotificationDispatch()
    {
    }

    private NotificationDispatch(
        Guid id,
        Guid storeId,
        NotificationChannel channel,
        NotificationTrigger trigger,
        string businessEntityType,
        Guid businessEntityId,
        Guid? customerId,
        string? recipientAddress,
        string? recipientName)
    {
        if (storeId == Guid.Empty)
            throw new NotificationDomainException("Store id is required.");

        if (businessEntityId == Guid.Empty)
            throw new NotificationDomainException("Business entity id is required.");

        Id = id;
        StoreId = storeId;
        Channel = channel;
        Trigger = trigger;
        BusinessEntityType = NormalizeRequired(businessEntityType, "Business entity type", 50);
        BusinessEntityId = businessEntityId;
        CustomerId = customerId;
        RecipientAddress = NormalizeOptional(recipientAddress, 320);
        RecipientName = NormalizeOptional(recipientName, 200);
        Status = NotificationStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationTrigger Trigger { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string? RecipientAddress { get; private set; }
    public string? RecipientName { get; private set; }
    public string? Subject { get; private set; }
    public string? Body { get; private set; }
    public string BusinessEntityType { get; private set; } = default!;
    public Guid BusinessEntityId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public string? ProviderName { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }
    public string? SuppressionReason { get; private set; }
    public string? LastProviderEventType { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? SentAtUtc { get; private set; }
    public DateTime? LastAttemptAtUtc { get; private set; }
    public DateTime? LastProviderEventAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public DateTime? OpenedAtUtc { get; private set; }
    public DateTime? ClickedAtUtc { get; private set; }
    public DateTime? BouncedAtUtc { get; private set; }
    public DateTime? ComplainedAtUtc { get; private set; }

    public IReadOnlyCollection<NotificationAttempt> Attempts => _attempts.AsReadOnly();

    public static NotificationDispatch Create(
        Guid storeId,
        NotificationChannel channel,
        NotificationTrigger trigger,
        string businessEntityType,
        Guid businessEntityId,
        Guid? customerId,
        string? recipientAddress,
        string? recipientName)
    {
        return new NotificationDispatch(
            Guid.NewGuid(),
            storeId,
            channel,
            trigger,
            businessEntityType,
            businessEntityId,
            customerId,
            recipientAddress,
            recipientName);
    }

    public void SetRenderedContent(string subject, string body)
    {
        EnsurePending();
        Subject = NormalizeRequired(subject, "Subject", 500);
        Body = NormalizeRequired(body, "Body", 20000);
        Touch();
    }

    public void MarkSuppressed(string reason)
    {
        EnsurePending();
        Status = NotificationStatus.Suppressed;
        SuppressionReason = NormalizeRequired(reason, "Suppression reason", 500);
        Touch();
    }

    public void MarkSent(
        string providerName,
        string? providerRequestReference,
        string? providerMessageId)
    {
        EnsurePending();
        EnsureRendered();

        ProviderName = NormalizeRequired(providerName, "Provider name", 100);
        ProviderMessageId = NormalizeOptional(providerMessageId, 200);
        FailureCode = null;
        FailureMessage = null;
        SuppressionReason = null;
        LastAttemptAtUtc = DateTime.UtcNow;
        SentAtUtc = LastAttemptAtUtc;
        Status = NotificationStatus.Sent;

        _attempts.Add(NotificationAttempt.CreateSent(
            Id,
            _attempts.Count + 1,
            ProviderName,
            providerRequestReference,
            ProviderMessageId));

        Touch();
    }

    public void MarkFailed(
        string providerName,
        string? providerRequestReference,
        string? failureCode,
        string? failureMessage)
    {
        EnsurePending();

        ProviderName = NormalizeRequired(providerName, "Provider name", 100);
        FailureCode = NormalizeOptional(failureCode, 100);
        FailureMessage = NormalizeOptional(failureMessage, 1000);
        LastAttemptAtUtc = DateTime.UtcNow;
        Status = NotificationStatus.Failed;

        _attempts.Add(NotificationAttempt.CreateFailed(
            Id,
            _attempts.Count + 1,
            ProviderName,
            providerRequestReference,
            FailureCode,
            FailureMessage));

        Touch();
    }

    public void RegisterDeliveredEvent(DateTime occurredAtUtc, string eventType)
    {
        EnsureSentOrFailed();
        DeliveredAtUtc ??= EnsureUtc(occurredAtUtc);
        SetProviderEvent(eventType, occurredAtUtc);
    }

    public void RegisterOpenedEvent(DateTime occurredAtUtc, string eventType)
    {
        EnsureSentOrFailed();
        OpenedAtUtc ??= EnsureUtc(occurredAtUtc);
        SetProviderEvent(eventType, occurredAtUtc);
    }

    public void RegisterClickedEvent(DateTime occurredAtUtc, string eventType)
    {
        EnsureSentOrFailed();
        ClickedAtUtc ??= EnsureUtc(occurredAtUtc);
        SetProviderEvent(eventType, occurredAtUtc);
    }

    public void RegisterProviderEvent(DateTime occurredAtUtc, string eventType)
    {
        EnsureSentOrFailed();
        SetProviderEvent(eventType, occurredAtUtc);
    }

    public void RegisterBouncedEvent(
        DateTime occurredAtUtc,
        string eventType,
        string? failureCode,
        string? failureMessage)
    {
        EnsureSentOrFailed();
        Status = NotificationStatus.Failed;
        BouncedAtUtc ??= EnsureUtc(occurredAtUtc);
        FailureCode = NormalizeOptional(failureCode, 100);
        FailureMessage = NormalizeOptional(failureMessage, 1000);
        SetProviderEvent(eventType, occurredAtUtc);
    }

    public void RegisterComplainedEvent(
        DateTime occurredAtUtc,
        string eventType,
        string? failureMessage)
    {
        EnsureSentOrFailed();
        Status = NotificationStatus.Failed;
        ComplainedAtUtc ??= EnsureUtc(occurredAtUtc);
        FailureMessage = NormalizeOptional(failureMessage, 1000);
        SetProviderEvent(eventType, occurredAtUtc);
    }

    private void EnsurePending()
    {
        if (Status != NotificationStatus.Pending)
            throw new NotificationDomainException("Only pending notification dispatch can be modified.");
    }

    private void EnsureSentOrFailed()
    {
        if (Status is not NotificationStatus.Sent and not NotificationStatus.Failed)
            throw new NotificationDomainException("Webhook events can only be applied to sent or failed dispatches.");
    }

    private void EnsureRendered()
    {
        if (string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Body))
            throw new NotificationDomainException("Notification content must be rendered before sending.");
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetProviderEvent(string eventType, DateTime occurredAtUtc)
    {
        LastProviderEventType = NormalizeRequired(eventType, "Provider event type", 100);
        LastProviderEventAtUtc = EnsureUtc(occurredAtUtc);
        Touch();
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new NotificationDomainException($"{fieldName} is required.");

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new NotificationDomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new NotificationDomainException($"Value cannot exceed {maxLength} characters.");

        return normalized;
    }
}
