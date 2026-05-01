namespace Payment.Application.Integrations;

public interface IOrderPaymentSyncService
{
    Task MarkAuthorizedAsync(Guid storeId, Guid orderId, string? paymentReference, CancellationToken cancellationToken = default);
    Task MarkCapturedAsync(Guid storeId, Guid orderId, string? paymentReference, CancellationToken cancellationToken = default);
    Task MarkFailedAsync(Guid storeId, Guid orderId, string? paymentReference, CancellationToken cancellationToken = default);
    Task MarkRefundedAsync(Guid storeId, Guid orderId, string? paymentReference, CancellationToken cancellationToken = default);
}
