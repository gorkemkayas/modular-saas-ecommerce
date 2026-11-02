using BuildingBlocks.Domain.Common;
using Payment.Domain.Events;
using Payment.Domain.Common;
using Payment.Domain.Enums;
using Payment.Domain.Exceptions;

namespace Payment.Domain.Entities;

public sealed class Payment : AggregateRoot, IAggregateRoot
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
        PaymentMethodType methodType,
        string? customerEmail = null,
        string? customerFullName = null)
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
        CustomerEmail = NormalizeOptional(customerEmail, 320);
        CustomerFullName = NormalizeOptional(customerFullName, 200);
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
    public string? CustomerEmail { get; private set; }
    public string? CustomerFullName { get; private set; }
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; } = default!;
    public PaymentStatus Status { get; private set; }
    public bool HasProviderAttempt { get; private set; }
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
        PaymentMethodType methodType,
        string? customerEmail = null,
        string? customerFullName = null)
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
            methodType,
            customerEmail,
            customerFullName);
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

    public bool RecordProviderAttemptStarted()
    {
        EnsureNotTerminalForPaymentOperation();
        if (HasProviderAttempt) return false;
        HasProviderAttempt = true;
        Touch();
        return true;
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
        var notify = Status != PaymentStatus.Authorized;
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
        if (notify) RaiseStatusEvent(PaymentEventKind.Authorized, Amount, null);
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
        RaiseStatusEvent(PaymentEventKind.Captured, Amount, null);
    }

    public void MarkFailed(
        string idempotencyKey,
        PaymentOperationType operationType,
        string? failureCode,
        string? failureMessage,
        string? providerRequestReference = null,
        string? providerTransactionReference = null)
    {
        if (operationType == PaymentOperationType.Refund)
        {
            RecordRefundFailure(idempotencyKey, failureCode, failureMessage,
                providerRequestReference, providerTransactionReference);
            return;
        }

        if (Status is PaymentStatus.Captured or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded or PaymentStatus.Cancelled)
            throw new PaymentDomainException("Captured, cancelled or refunded payment cannot be marked failed for this operation.");

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
        RaiseStatusEvent(PaymentEventKind.Failed, Amount, FailureMessage);
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
        var existing = GetRefundByIdempotencyKey(idempotencyKey);
        if (existing is not null)
        {
            if (existing.Amount != amount)
                throw new PaymentDomainException("The refund key was already used for a different amount.");
            return;
        }
        ValidateRefundRequest(idempotencyKey, amount, reason);
        var remainingAmount = Amount - RefundedAmount;
        var refund = PaymentRefund.Create(Id, amount, reason, providerRefundReference, idempotencyKey.Trim());
        _refunds.Add(refund);

        Status = refund.Amount == remainingAmount
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
        RaiseStatusEvent(PaymentEventKind.Refunded, refund.Amount, reason, refund.Id);
    }

    public PaymentRefund? GetRefundByIdempotencyKey(string idempotencyKey)
    {
        var key = NormalizeRequired(idempotencyKey, "Idempotency key", 120);
        var refund = _refunds.SingleOrDefault(x => x.IdempotencyKey == key);
        if (refund is not null) return refund;

        // Older refunds lack the key column. Only use legacy history when it
        // identifies exactly one result; never guess and issue another refund.
        var attempt = _attempts.LastOrDefault(x => x.OperationType == PaymentOperationType.Refund &&
            x.Status == PaymentAttemptStatus.Succeeded && x.IdempotencyKey == key);
        if (attempt is null) return null;
        var matches = _refunds.Where(x => x.IdempotencyKey is null &&
            x.ProviderRefundReference == attempt.ProviderTransactionReference).ToArray();
        return matches.Length == 1 ? matches[0] :
            throw new PaymentDomainException("The prior refund result is ambiguous and requires reconciliation.");
    }

    public void ValidateRefundRequest(string idempotencyKey, decimal amount, string reason)
    {
        var key = NormalizeRequired(idempotencyKey, "Idempotency key", 120);
        var normalizedReason = NormalizeRequired(reason, "Refund reason", 500);
        var normalizedAmount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        if (normalizedAmount <= 0 || normalizedAmount != amount)
            throw new PaymentDomainException("Refund amount must be positive with at most two decimal places.");
        var existing = GetRefundByIdempotencyKey(key);
        if (existing is not null)
        {
            if (existing.Amount != normalizedAmount || existing.Reason != normalizedReason)
                throw new PaymentDomainException("The refund key was already used for a different request.");
            return;
        }
        if (Status is not PaymentStatus.Captured and not PaymentStatus.PartiallyRefunded)
            throw new PaymentDomainException("Only captured payment can be refunded.");
        if (normalizedAmount > Amount - RefundedAmount)
            throw new PaymentDomainException("Refund amount cannot exceed remaining captured amount.");
    }

    public void RecordRefundFailure(string idempotencyKey, string? failureCode, string? failureMessage,
        string? providerRequestReference = null, string? providerTransactionReference = null)
    {
        if (GetRefundByIdempotencyKey(idempotencyKey) is not null) return;
        if (Status is not PaymentStatus.Captured and not PaymentStatus.PartiallyRefunded)
            throw new PaymentDomainException("Refund failure requires a captured payment.");
        AddAttempt(PaymentOperationType.Refund, PaymentAttemptStatus.Failed, idempotencyKey,
            providerRequestReference, providerTransactionReference, failureCode, failureMessage);
        Touch();
    }

    private void RaiseStatusEvent(PaymentEventKind kind, decimal amount, string? description, Guid? refundId = null)
    {
        RaiseDomainEvent(new PaymentStatusChangedDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, StoreId, OrderId, CustomerId,
            OrderNumber, kind, amount, CurrencyCode, ExternalPaymentReference, description, refundId,
            CustomerEmail, CustomerFullName));
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
        idempotencyKey = NormalizeRequired(idempotencyKey, "Idempotency key", 120);
        var existingAttempt = _attempts.LastOrDefault(x => x.IdempotencyKey == idempotencyKey && x.OperationType == operationType);

        if (existingAttempt is not null && existingAttempt.Status == status)
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
