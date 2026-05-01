namespace Order.Contracts;

public interface IOrderModuleApi
{
    Task<OrderPaymentContextResult?> GetCustomerOrderPaymentContextAsync(
        GetCustomerOrderPaymentContextRequest request,
        CancellationToken cancellationToken = default);

    Task<OrderPaymentContextResult?> GetStoreOrderPaymentContextAsync(
        GetStoreOrderPaymentContextRequest request,
        CancellationToken cancellationToken = default);

    Task MarkPaymentAuthorizedAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken = default);

    Task MarkPaymentCapturedAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken = default);

    Task MarkPaymentFailedAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken = default);

    Task MarkPaymentRefundedAsync(
        UpdateOrderPaymentStatusRequest request,
        CancellationToken cancellationToken = default);
}
