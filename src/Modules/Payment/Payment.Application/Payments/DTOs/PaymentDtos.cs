using Payment.Domain.Enums;

namespace Payment.Application.Payments.DTOs;

public sealed record PaymentActionResultDto(
    Guid PaymentId,
    Guid StoreId,
    Guid OrderId,
    PaymentStatus Status,
    string? ExternalPaymentReference,
    string? ExternalConversationId,
    string? ActionUrl,
    string? FailureCode,
    string? FailureMessage);

public sealed record PaymentAttemptDto(
    Guid Id,
    int AttemptNumber,
    PaymentOperationType OperationType,
    PaymentAttemptStatus Status,
    string IdempotencyKey,
    string? ProviderRequestReference,
    string? ProviderTransactionReference,
    string? FailureCode,
    string? FailureMessage,
    DateTime ProcessedAtUtc);

public sealed record PaymentRefundDto(
    Guid Id,
    decimal Amount,
    string Reason,
    string? ProviderRefundReference,
    DateTime CreatedAtUtc);

public sealed record PaymentDto(
    Guid Id,
    Guid StoreId,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    decimal Amount,
    string CurrencyCode,
    PaymentStatus Status,
    PaymentProvider Provider,
    Guid? ProviderAccountId,
    PaymentMethodType MethodType,
    string? ExternalPaymentReference,
    string? ExternalConversationId,
    string? FailureCode,
    string? FailureMessage,
    DateTime? AuthorizedAtUtc,
    DateTime? CapturedAtUtc,
    DateTime? CancelledAtUtc,
    DateTime? FailedAtUtc,
    decimal RefundedAmount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<PaymentAttemptDto> Attempts,
    IReadOnlyCollection<PaymentRefundDto> Refunds);

public sealed record PaymentSummaryDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string CurrencyCode,
    PaymentStatus Status,
    PaymentProvider Provider,
    Guid? ProviderAccountId,
    PaymentMethodType MethodType,
    DateTime CreatedAtUtc);
