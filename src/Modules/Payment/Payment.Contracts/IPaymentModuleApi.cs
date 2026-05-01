namespace Payment.Contracts;

public interface IPaymentModuleApi
{
    Task<Guid> CreateForOrderAsync(
        CreatePaymentForOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentOperationResult> AuthorizeAsync(
        AuthorizePaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentOperationResult> CaptureAsync(
        CapturePaymentRequest request,
        CancellationToken cancellationToken = default);

    Task CancelAsync(
        CancelPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<RefundPaymentResult> RefundAsync(
        RefundPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentStatusResult?> GetByOrderIdAsync(
        GetPaymentByOrderIdRequest request,
        CancellationToken cancellationToken = default);
}
