using Payment.Domain.Enums;

namespace Payment.Application.Integrations;

public interface IPaymentGateway
{
    PaymentProvider Provider { get; }

    Task<PaymentGatewayOperationResult> AuthorizeAsync(
        PaymentGatewayAuthorizeRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentGatewayOperationResult> CompleteAsync(
        PaymentGatewayCompleteRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentGatewayOperationResult> CaptureAsync(
        PaymentGatewayCaptureRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentGatewayOperationResult> CancelAsync(
        PaymentGatewayCancelRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentGatewayOperationResult> RefundAsync(
        PaymentGatewayRefundRequest request,
        CancellationToken cancellationToken = default);
}
