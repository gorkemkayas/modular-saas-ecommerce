using MediatR;
using Payment.Application.Abstractions.Queries;
using Payment.Application.Payments.Commands.AuthorizePayment;
using Payment.Application.Payments.Commands.CancelPayment;
using Payment.Application.Payments.Commands.CapturePayment;
using Payment.Application.Payments.Commands.CreatePaymentForOrder;
using Payment.Application.Payments.Commands.RefundPayment;
using Payment.Contracts;

namespace Payment.Application.Contracts;

public sealed class PaymentModuleApi : IPaymentModuleApi
{
    private readonly ISender _sender;
    private readonly IPaymentReadService _paymentReadService;

    public PaymentModuleApi(
        ISender sender,
        IPaymentReadService paymentReadService)
    {
        _sender = sender;
        _paymentReadService = paymentReadService;
    }

    public Task<Guid> CreateForOrderAsync(
        CreatePaymentForOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        return _sender.Send(new CreatePaymentForOrderCommand(
            request.StoreId,
            request.ExternalUserId,
            request.OrderId,
            request.MethodType), cancellationToken);
    }

    public async Task<PaymentOperationResult> AuthorizeAsync(
        AuthorizePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new AuthorizePaymentCommand(
            request.StoreId,
            request.ExternalUserId,
            request.OrderId,
            request.IdempotencyKey,
            request.ClientIpAddress), cancellationToken);

        return new PaymentOperationResult(
            result.PaymentId,
            result.Status,
            result.ExternalPaymentReference,
            result.ExternalConversationId,
            result.ActionUrl,
            result.FailureCode,
            result.FailureMessage);
    }

    public async Task<PaymentOperationResult> CaptureAsync(
        CapturePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new CapturePaymentCommand(
            request.StoreId,
            request.PaymentId,
            request.IdempotencyKey), cancellationToken);

        return new PaymentOperationResult(
            result.PaymentId,
            result.Status,
            result.ExternalPaymentReference,
            result.ExternalConversationId,
            result.ActionUrl,
            result.FailureCode,
            result.FailureMessage);
    }

    public Task CancelAsync(
        CancelPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        return _sender.Send(new CancelPaymentCommand(
            request.StoreId,
            request.PaymentId,
            request.IdempotencyKey), cancellationToken);
    }

    public Task<RefundPaymentResult> RefundAsync(
        RefundPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        return _sender.Send(new RefundPaymentCommand(
            request.StoreId,
            request.PaymentId,
            request.Amount,
            request.Reason,
            request.IdempotencyKey), cancellationToken);
    }

    public async Task<PaymentStatusResult?> GetByOrderIdAsync(
        GetPaymentByOrderIdRequest request,
        CancellationToken cancellationToken = default)
    {
        var payment = await _paymentReadService.GetByOrderIdAsync(
            request.StoreId,
            request.OrderId,
            cancellationToken);

        return payment is null
            ? null
            : new PaymentStatusResult(
                payment.Id,
                payment.OrderId,
                payment.Status,
                payment.Amount,
                payment.CurrencyCode,
                payment.ExternalPaymentReference,
                payment.ExternalConversationId);
    }
}
