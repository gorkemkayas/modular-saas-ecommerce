using BuildingBlocks.Domain.Events;

namespace Payment.Domain.Events;

public enum PaymentEventKind { Authorized = 1, Captured = 2, Failed = 3, Refunded = 4 }

public sealed record PaymentStatusChangedDomainEvent(
    Guid Id, DateTime OccurredOnUtc, Guid PaymentId, Guid StoreId, Guid OrderId,
    Guid CustomerId, string OrderNumber, PaymentEventKind Kind, decimal Amount,
    string CurrencyCode, string? PaymentReference, string? Description,
    Guid? RefundId = null, string? RecipientEmail = null, string? RecipientName = null) : IDomainEvent;
