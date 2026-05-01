using Payment.Domain.Enums;
using Payment.Domain.Exceptions;

namespace Payment.Domain.Entities;

public sealed class PaymentAttempt
{
    private PaymentAttempt()
    {
    }

    private PaymentAttempt(
        Guid id,
        Guid paymentId,
        int attemptNumber,
        PaymentOperationType operationType,
        PaymentAttemptStatus status,
        string idempotencyKey,
        string? providerRequestReference,
        string? providerTransactionReference,
        string? failureCode,
        string? failureMessage)
    {
        if (paymentId == Guid.Empty)
            throw new PaymentDomainException("Payment id is required.");

        if (attemptNumber <= 0)
            throw new PaymentDomainException("Attempt number must be greater than zero.");

        Id = id;
        PaymentId = paymentId;
        AttemptNumber = attemptNumber;
        OperationType = operationType;
        Status = status;
        IdempotencyKey = NormalizeRequired(idempotencyKey, "Idempotency key", 120);
        ProviderRequestReference = NormalizeOptional(providerRequestReference, 200);
        ProviderTransactionReference = NormalizeOptional(providerTransactionReference, 200);
        FailureCode = NormalizeOptional(failureCode, 100);
        FailureMessage = NormalizeOptional(failureMessage, 500);
        ProcessedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public int AttemptNumber { get; private set; }
    public PaymentOperationType OperationType { get; private set; }
    public PaymentAttemptStatus Status { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public string? ProviderRequestReference { get; private set; }
    public string? ProviderTransactionReference { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }
    public DateTime ProcessedAtUtc { get; private set; }

    public static PaymentAttempt Create(
        Guid paymentId,
        int attemptNumber,
        PaymentOperationType operationType,
        PaymentAttemptStatus status,
        string idempotencyKey,
        string? providerRequestReference,
        string? providerTransactionReference,
        string? failureCode,
        string? failureMessage)
    {
        return new PaymentAttempt(
            Guid.NewGuid(),
            paymentId,
            attemptNumber,
            operationType,
            status,
            idempotencyKey,
            providerRequestReference,
            providerTransactionReference,
            failureCode,
            failureMessage);
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PaymentDomainException($"{fieldName} is required.");

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new PaymentDomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new PaymentDomainException($"Value cannot exceed {maxLength} characters.");

        return normalized;
    }
}
