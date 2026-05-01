using Payment.Application.Integrations;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Gateways;

public sealed class MockPaymentGateway : IPaymentGateway
{
    public PaymentProvider Provider => PaymentProvider.Mock;

    public Task<PaymentGatewayOperationResult> AuthorizeAsync(
        PaymentGatewayAuthorizeRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(BuildResult(
            request.IdempotencyKey,
            $"mock-pay-{request.PaymentId:N}",
            $"mock-conv-{request.PaymentId:N}"));
    }

    public Task<PaymentGatewayOperationResult> CompleteAsync(
        PaymentGatewayCompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        var outcome = ResolveOutcome(request.Token);
        var conversationId = request.Token.Replace("mock-token-", "mock-conv-", StringComparison.OrdinalIgnoreCase);
        var paymentReference = request.Token.Replace("mock-token-", "mock-pay-", StringComparison.OrdinalIgnoreCase);

        return Task.FromResult(new PaymentGatewayOperationResult(
            outcome == PaymentGatewayOutcome.RequiresAction ? PaymentGatewayOutcome.Captured : outcome,
            paymentReference,
            conversationId,
            null,
            outcome == PaymentGatewayOutcome.Failed ? "mock_payment_failed" : null,
            outcome == PaymentGatewayOutcome.Failed ? "Mock payment failure requested." : null,
            request.Token));
    }

    public Task<PaymentGatewayOperationResult> CaptureAsync(
        PaymentGatewayCaptureRequest request,
        CancellationToken cancellationToken = default)
    {
        var outcome = request.IdempotencyKey.Contains("fail", StringComparison.OrdinalIgnoreCase)
            ? PaymentGatewayOutcome.Failed
            : PaymentGatewayOutcome.Captured;

        return Task.FromResult(BuildResult(
            request.IdempotencyKey,
            request.ExternalPaymentReference ?? $"mock-pay-{request.PaymentId:N}",
            request.ExternalConversationId ?? $"mock-conv-{request.PaymentId:N}",
            forcedOutcome: outcome));
    }

    public Task<PaymentGatewayOperationResult> CancelAsync(
        PaymentGatewayCancelRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PaymentGatewayOperationResult(
            PaymentGatewayOutcome.Cancelled,
            request.ExternalPaymentReference ?? $"mock-pay-{request.PaymentId:N}",
            request.ExternalConversationId ?? $"mock-conv-{request.PaymentId:N}",
            null,
            null,
            null,
            $"mock-cancel-{request.PaymentId:N}"));
    }

    public Task<PaymentGatewayOperationResult> RefundAsync(
        PaymentGatewayRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        var outcome = request.IdempotencyKey.Contains("fail", StringComparison.OrdinalIgnoreCase)
            ? PaymentGatewayOutcome.Failed
            : PaymentGatewayOutcome.Refunded;

        return Task.FromResult(new PaymentGatewayOperationResult(
            outcome,
            request.ExternalPaymentReference ?? $"mock-refund-{request.PaymentId:N}",
            request.ExternalConversationId ?? $"mock-conv-{request.PaymentId:N}",
            null,
            outcome == PaymentGatewayOutcome.Failed ? "mock_refund_failed" : null,
            outcome == PaymentGatewayOutcome.Failed ? "Mock refund failure requested." : null,
            $"mock-refund-{request.PaymentId:N}",
            outcome == PaymentGatewayOutcome.Refunded ? request.RefundAmount : null));
    }

    private static PaymentGatewayOperationResult BuildResult(
        string idempotencyKey,
        string paymentReference,
        string conversationId,
        PaymentGatewayOutcome? forcedOutcome = null)
    {
        var outcome = forcedOutcome ?? ResolveOutcome(idempotencyKey);

        return outcome switch
        {
            PaymentGatewayOutcome.RequiresAction => new PaymentGatewayOperationResult(
                outcome,
                paymentReference,
                conversationId,
                $"https://mock-payments.local/3ds/{conversationId}",
                null,
                null,
                $"mock-token-{conversationId}"),
            PaymentGatewayOutcome.Failed => new PaymentGatewayOperationResult(
                outcome,
                paymentReference,
                conversationId,
                null,
                "mock_payment_failed",
                "Mock payment failure requested.",
                $"mock-token-{conversationId}"),
            _ => new PaymentGatewayOperationResult(
                outcome,
                paymentReference,
                conversationId,
                null,
                null,
                null,
                $"mock-token-{conversationId}")
        };
    }

    private static PaymentGatewayOutcome ResolveOutcome(string idempotencyKey)
    {
        if (idempotencyKey.Contains("fail", StringComparison.OrdinalIgnoreCase))
            return PaymentGatewayOutcome.Failed;

        if (idempotencyKey.Contains("action", StringComparison.OrdinalIgnoreCase))
            return PaymentGatewayOutcome.RequiresAction;

        if (idempotencyKey.Contains("capture", StringComparison.OrdinalIgnoreCase))
            return PaymentGatewayOutcome.Captured;

        return PaymentGatewayOutcome.Authorized;
    }
}
