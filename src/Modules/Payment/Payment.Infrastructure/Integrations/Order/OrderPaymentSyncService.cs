using Order.Contracts;
using Payment.Application.Integrations;

namespace Payment.Infrastructure.Integrations.Order;

public sealed class OrderPaymentSyncService : IOrderPaymentSyncService
{
    private readonly IOrderModuleApi _orderModuleApi;

    public OrderPaymentSyncService(IOrderModuleApi orderModuleApi)
    {
        _orderModuleApi = orderModuleApi;
    }

    public Task MarkAuthorizedAsync(Guid storeId, Guid orderId, string? paymentReference, CancellationToken cancellationToken = default)
    {
        return _orderModuleApi.MarkPaymentAuthorizedAsync(
            new UpdateOrderPaymentStatusRequest(storeId, orderId, paymentReference),
            cancellationToken);
    }

    public Task MarkCapturedAsync(Guid storeId, Guid orderId, string? paymentReference, CancellationToken cancellationToken = default)
    {
        return _orderModuleApi.MarkPaymentCapturedAsync(
            new UpdateOrderPaymentStatusRequest(storeId, orderId, paymentReference),
            cancellationToken);
    }

    public Task MarkFailedAsync(Guid storeId, Guid orderId, string? paymentReference, CancellationToken cancellationToken = default)
    {
        return _orderModuleApi.MarkPaymentFailedAsync(
            new UpdateOrderPaymentStatusRequest(storeId, orderId, paymentReference),
            cancellationToken);
    }

    public Task MarkRefundedAsync(Guid storeId, Guid orderId, string? paymentReference, CancellationToken cancellationToken = default)
    {
        return _orderModuleApi.MarkPaymentRefundedAsync(
            new UpdateOrderPaymentStatusRequest(storeId, orderId, paymentReference),
            cancellationToken);
    }
}
