using Notification.Domain.Enums;
using Notification.Domain.Exceptions;

namespace Notification.Domain.Entities;

public sealed class NotificationAttempt
{
    private NotificationAttempt()
    {
    }

    private NotificationAttempt(
        Guid id,
        Guid dispatchId,
        int attemptNumber,
        NotificationAttemptStatus status,
        string providerName,
        string? providerRequestReference,
        string? providerMessageId,
        string? failureCode,
        string? failureMessage)
    {
        if (dispatchId == Guid.Empty)
            throw new NotificationDomainException("Dispatch id is required.");

        if (attemptNumber <= 0)
            throw new NotificationDomainException("Attempt number must be greater than zero.");

        Id = id;
        NotificationDispatchId = dispatchId;
        AttemptNumber = attemptNumber;
        Status = status;
        ProviderName = NormalizeRequired(providerName, "Provider name", 100);
        ProviderRequestReference = NormalizeOptional(providerRequestReference, 200);
        ProviderMessageId = NormalizeOptional(providerMessageId, 200);
        FailureCode = NormalizeOptional(failureCode, 100);
        FailureMessage = NormalizeOptional(failureMessage, 1000);
        AttemptedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid NotificationDispatchId { get; private set; }
    public int AttemptNumber { get; private set; }
    public NotificationAttemptStatus Status { get; private set; }
    public string ProviderName { get; private set; } = default!;
    public string? ProviderRequestReference { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }
    public DateTime AttemptedAtUtc { get; private set; }

    internal static NotificationAttempt CreateSent(
        Guid dispatchId,
        int attemptNumber,
        string providerName,
        string? providerRequestReference,
        string? providerMessageId)
    {
        return new NotificationAttempt(
            Guid.NewGuid(),
            dispatchId,
            attemptNumber,
            NotificationAttemptStatus.Sent,
            providerName,
            providerRequestReference,
            providerMessageId,
            null,
            null);
    }

    internal static NotificationAttempt CreateFailed(
        Guid dispatchId,
        int attemptNumber,
        string providerName,
        string? providerRequestReference,
        string? failureCode,
        string? failureMessage)
    {
        return new NotificationAttempt(
            Guid.NewGuid(),
            dispatchId,
            attemptNumber,
            NotificationAttemptStatus.Failed,
            providerName,
            providerRequestReference,
            null,
            failureCode,
            failureMessage);
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
