using BuildingBlocks.Messaging.Abstractions.Events;

namespace Payment.Contracts.IntegrationEvents;

public enum PaymentEventKind { Authorized = 1, Captured = 2, Failed = 3, Refunded = 4 }

public sealed record PaymentStatusChangedIntegrationEvent(
    Guid Id, DateTime OccurredOnUtc, Guid PaymentId, Guid StoreId, Guid OrderId,
    Guid CustomerId, string OrderNumber, PaymentEventKind Kind, decimal Amount,
    string CurrencyCode, string? PaymentReference, string? Description,
    string? RecipientEmail, string? RecipientName, Guid? RefundId = null) : IIntegrationEvent
{
    public const string EventType = "payment.status-changed.v1";
    string IIntegrationEvent.EventType => EventType;
}
