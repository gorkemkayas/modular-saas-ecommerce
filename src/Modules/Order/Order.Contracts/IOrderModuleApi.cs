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

    Task<OrderShipmentContextResult?> GetCustomerOrderShipmentContextAsync(
        GetCustomerOrderShipmentContextRequest request,
        CancellationToken cancellationToken = default);

    Task<OrderShipmentContextResult?> GetStoreOrderShipmentContextAsync(
        GetStoreOrderShipmentContextRequest request,
        CancellationToken cancellationToken = default);

    Task MarkShipmentCreatedAsync(
        UpdateOrderShipmentStatusRequest request,
        CancellationToken cancellationToken = default);

    Task MarkShippedAsync(
        UpdateOrderShipmentStatusRequest request,
        CancellationToken cancellationToken = default);

    Task MarkDeliveredAsync(
        UpdateOrderShipmentStatusRequest request,
        CancellationToken cancellationToken = default);

    Task MarkShipmentCancelledAsync(
        UpdateOrderShipmentStatusRequest request,
        CancellationToken cancellationToken = default);
}
