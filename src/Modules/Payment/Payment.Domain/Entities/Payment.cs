using Payment.Domain.Common;
using Payment.Domain.Enums;
using Payment.Domain.Exceptions;

namespace Payment.Domain.Entities;

public sealed class Payment : IAggregateRoot
{
    private readonly List<PaymentAttempt> _attempts = new();
    private readonly List<PaymentRefund> _refunds = new();

    private Payment()
    {
    }

    private Payment(
        Guid id,
        Guid storeId,
        Guid orderId,
        string orderNumber,
        Guid customerId,
        decimal amount,
        string currencyCode,
        PaymentProvider provider,
        PaymentMethodType methodType)
    {
        if (storeId == Guid.Empty)
            throw new PaymentDomainException("Store id is required.");

        if (orderId == Guid.Empty)
            throw new PaymentDomainException("Order id is required.");

        if (customerId == Guid.Empty)
            throw new PaymentDomainException("Customer id is required.");

        if (amount <= 0)
            throw new PaymentDomainException("Payment amount must be greater than zero.");

        Id = id;
        StoreId = storeId;
        OrderId = orderId;
        OrderNumber = NormalizeRequired(orderNumber, "Order number", 50);
        CustomerId = customerId;
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        CurrencyCode = NormalizeCurrency(currencyCode);
        Provider = provider;
        MethodType = methodType;
        Status = PaymentStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid OrderId { get; private set; }
    public string OrderNumber { get; private set; } = default!;
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; } = default!;
    public PaymentStatus Status { get; private set; }
    public PaymentProvider Provider { get; private set; }
    public Guid? ProviderAccountId { get; private set; }
    public PaymentMethodType MethodType { get; private set; }
    public string? ExternalPaymentReference { get; private set; }
    public string? ExternalConversationId { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }
    public DateTime? AuthorizedAtUtc { get; private set; }
    public DateTime? CapturedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? FailedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public decimal RefundedAmount => _refunds.Sum(x => x.Amount);

    public IReadOnlyCollection<PaymentAttempt> Attempts => _attempts.AsReadOnly();
    public IReadOnlyCollection<PaymentRefund> Refunds => _refunds.AsReadOnly();

    public static Payment Create(
        Guid storeId,
        Guid orderId,
        string orderNumber,
        Guid customerId,
        decimal amount,
        string currencyCode,
        PaymentProvider provider,
        PaymentMethodType methodType)
    {
        return new Payment(
            Guid.NewGuid(),
            storeId,
            orderId,
            orderNumber,
            customerId,
            amount,
            currencyCode,
            provider,
            methodType);
    }

    public void AssignProviderAccount(Guid providerAccountId)
    {
        if (providerAccountId == Guid.Empty)
            throw new PaymentDomainException("Provider account id is required.");

        if (ProviderAccountId.HasValue && ProviderAccountId.Value != providerAccountId)
            throw new PaymentDomainException("Payment provider account cannot be changed after it is assigned.");

        ProviderAccountId = providerAccountId;
        Touch();
    }

    public void MarkRequiresAction(
        string idempotencyKey,
        PaymentOperationType operationType,
        string? externalConversationId,
        string? externalPaymentReference,
        string? providerRequestReference = null)
    {
        EnsureNotTerminalForPaymentOperation();
        ExternalConversationId = NormalizeOptional(externalConversationId, 200);
        ExternalPaymentReference = NormalizeOptional(externalPaymentReference, 200);
        Status = PaymentStatus.RequiresAction;
        FailureCode = null;
        FailureMessage = null;
        AddAttempt(
            operationType,
            PaymentAttemptStatus.Succeeded,
            idempotencyKey,
            providerRequestReference,
            externalPaymentReference,
            null,
            null);
        Touch();
    }

    public void MarkAuthorized(
        string idempotencyKey,
        PaymentOperationType operationType,
        string? externalConversationId,
        string? externalPaymentReference,
        string? providerRequestReference = null)
    {
        EnsureCanTransitionToAuthorized();
        ExternalConversationId = NormalizeOptional(externalConversationId, 200);
        ExternalPaymentReference = NormalizeOptional(externalPaymentReference, 200);
        Status = PaymentStatus.Authorized;
        AuthorizedAtUtc = DateTime.UtcNow;
        FailureCode = null;
        FailureMessage = null;
        AddAttempt(
            operationType,
            PaymentAttemptStatus.Succeeded,
            idempotencyKey,
            providerRequestReference,
            externalPaymentReference,
            null,
            null);
        Touch();
    }

    public void MarkCaptured(
        string idempotencyKey,
        string? externalConversationId,
        string? externalPaymentReference,
        string? providerRequestReference = null)
    {
        if (Status == PaymentStatus.Captured)
            return;

        if (Status is PaymentStatus.Cancelled or PaymentStatus.Refunded or PaymentStatus.PartiallyRefunded)
            throw new PaymentDomainException("Cancelled or refunded payment cannot be captured.");

        ExternalConversationId = NormalizeOptional(externalConversationId, 200);
        ExternalPaymentReference = NormalizeOptional(externalPaymentReference, 200);
        Status = PaymentStatus.Captured;
        CapturedAtUtc = DateTime.UtcNow;
        FailureCode = null;
        FailureMessage = null;
        AddAttempt(
            PaymentOperationType.Capture,
            PaymentAttemptStatus.Succeeded,
            idempotencyKey,
            providerRequestReference,
            externalPaymentReference,
            null,
            null);
        Touch();
    }

    public void MarkFailed(
        string idempotencyKey,
        PaymentOperationType operationType,
        string? failureCode,
        string? failureMessage,
        string? providerRequestReference = null,
        string? providerTransactionReference = null)
    {
        if (Status == PaymentStatus.Captured && operationType != PaymentOperationType.Refund)
            throw new PaymentDomainException("Captured payment cannot be marked failed for this operation.");

        Status = PaymentStatus.Failed;
        FailureCode = NormalizeOptional(failureCode, 100);
        FailureMessage = NormalizeOptional(failureMessage, 500);
        FailedAtUtc = DateTime.UtcNow;
        AddAttempt(
            operationType,
            PaymentAttemptStatus.Failed,
            idempotencyKey,
            providerRequestReference,
            providerTransactionReference,
            failureCode,
            failureMessage);
        Touch();
    }

    public void Cancel(
        string idempotencyKey,
        string? externalPaymentReference,
        string? providerRequestReference = null)
    {
        if (Status == PaymentStatus.Captured)
            throw new PaymentDomainException("Captured payment cannot be cancelled.");

        if (Status == PaymentStatus.Cancelled)
            return;

        if (Status is PaymentStatus.Refunded or PaymentStatus.PartiallyRefunded)
            throw new PaymentDomainException("Refunded payment cannot be cancelled.");

        ExternalPaymentReference = NormalizeOptional(externalPaymentReference, 200) ?? ExternalPaymentReference;
        Status = PaymentStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        AddAttempt(
            PaymentOperationType.Cancel,
            PaymentAttemptStatus.Succeeded,
            idempotencyKey,
            providerRequestReference,
            externalPaymentReference,
            null,
            null);
        Touch();
    }

    public void Refund(
        string idempotencyKey,
        decimal amount,
        string reason,
        string? providerRefundReference,
        string? providerRequestReference = null)
    {
        if (Status is not PaymentStatus.Captured and not PaymentStatus.PartiallyRefunded)
            throw new PaymentDomainException("Only captured payment can be refunded.");

        var remainingAmount = Amount - RefundedAmount;

        if (amount > remainingAmount)
            throw new PaymentDomainException("Refund amount cannot exceed remaining captured amount.");

        _refunds.Add(PaymentRefund.Create(Id, amount, reason, providerRefundReference));

        Status = amount == remainingAmount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;

        AddAttempt(
            PaymentOperationType.Refund,
            PaymentAttemptStatus.Succeeded,
            idempotencyKey,
            providerRequestReference,
            providerRefundReference,
            null,
            null);
        Touch();
    }

    private void EnsureNotTerminalForPaymentOperation()
    {
        if (Status is PaymentStatus.Cancelled or PaymentStatus.Refunded)
            throw new PaymentDomainException("Terminal payment cannot be modified.");
    }

    private void EnsureCanTransitionToAuthorized()
    {
        if (Status == PaymentStatus.Captured)
            throw new PaymentDomainException("Captured payment cannot be authorized again.");

        if (Status is PaymentStatus.Cancelled or PaymentStatus.Refunded or PaymentStatus.PartiallyRefunded)
            throw new PaymentDomainException("Cancelled or refunded payment cannot be authorized.");
    }

    private void AddAttempt(
        PaymentOperationType operationType,
        PaymentAttemptStatus status,
        string idempotencyKey,
        string? providerRequestReference,
        string? providerTransactionReference,
        string? failureCode,
        string? failureMessage)
    {
        var existingAttempt = _attempts.FirstOrDefault(x => x.IdempotencyKey == idempotencyKey && x.OperationType == operationType);

        if (existingAttempt is not null)
            return;

        _attempts.Add(PaymentAttempt.Create(
            Id,
            _attempts.Count + 1,
            operationType,
            status,
            idempotencyKey,
            providerRequestReference,
            providerTransactionReference,
            failureCode,
            failureMessage));
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
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

    private static string NormalizeCurrency(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PaymentDomainException("Currency code is required.");

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != 3)
            throw new PaymentDomainException("Currency code must be 3 characters.");

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
