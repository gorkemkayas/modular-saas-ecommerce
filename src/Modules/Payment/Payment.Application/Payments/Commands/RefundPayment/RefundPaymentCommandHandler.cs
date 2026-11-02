using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Application.Integrations;
using Payment.Contracts;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.Payments.Commands.RefundPayment;

public sealed class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, RefundPaymentResult>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderPaymentSyncService _orderPaymentSyncService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IPaymentRefundLock _refundLock;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IOrderPaymentSyncService orderPaymentSyncService,
        IPaymentGateway paymentGateway,
        IPaymentRefundLock refundLock,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _orderPaymentSyncService = orderPaymentSyncService;
        _paymentGateway = paymentGateway;
        _refundLock = refundLock;
        _logger = logger;
    }

    public async Task<RefundPaymentResult> Handle(RefundPaymentCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty || command.PaymentId == Guid.Empty)
            throw new PaymentValidationException("StoreId and PaymentId are required.");

        if (command.Amount <= 0)
            throw new PaymentValidationException("Refund amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(command.Reason) || string.IsNullOrWhiteSpace(command.IdempotencyKey))
            throw new PaymentValidationException("Reason and IdempotencyKey are required.");

        command = command with { IdempotencyKey = command.IdempotencyKey.Trim(), Reason = command.Reason.Trim() };
        await using var refundLease = await _refundLock.AcquireAsync(command.StoreId, command.PaymentId, cancellationToken);
        var payment = await _paymentRepository.GetByIdAsync(command.StoreId, command.PaymentId, cancellationToken)
            ?? throw new PaymentNotFoundException(command.PaymentId);

        payment.ValidateRefundRequest(command.IdempotencyKey, command.Amount, command.Reason);
        if (payment.GetRefundByIdempotencyKey(command.IdempotencyKey) is not null)
        {
            await SyncFullRefundAsync(payment, cancellationToken);
            return new RefundPaymentResult(payment.Id, payment.Status, payment.RefundedAmount);
        }

        var gatewayResult = await _paymentGateway.RefundAsync(
            new PaymentGatewayRefundRequest(
                payment.Id,
                payment.StoreId,
                payment.OrderId,
                payment.Amount,
                command.Amount,
                payment.CurrencyCode,
                command.Reason,
                payment.ExternalPaymentReference,
                payment.ExternalConversationId,
                command.IdempotencyKey,
                payment.ProviderAccountId),
            cancellationToken);

        if (gatewayResult.ProviderAccountId.HasValue)
            payment.AssignProviderAccount(gatewayResult.ProviderAccountId.Value);

        if (gatewayResult.Outcome != PaymentGatewayOutcome.Refunded)
        {
            payment.RecordRefundFailure(
                command.IdempotencyKey,
                gatewayResult.FailureCode,
                gatewayResult.FailureMessage,
                gatewayResult.ProviderRequestReference,
                gatewayResult.ExternalPaymentReference);
        }
        else
        {
            payment.Refund(
                command.IdempotencyKey,
                command.Amount,
                command.Reason,
                gatewayResult.ExternalPaymentReference,
                gatewayResult.ProviderRequestReference);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await SyncFullRefundAsync(payment, cancellationToken);

        _logger.LogInformation(
            "Payment refund handled | PaymentId: {PaymentId} | Status: {Status} | RefundedAmount: {RefundedAmount}",
            payment.Id,
            payment.Status,
            payment.RefundedAmount);

        return new RefundPaymentResult(payment.Id, payment.Status, payment.RefundedAmount);
    }

    private Task SyncFullRefundAsync(Payment.Domain.Entities.Payment payment, CancellationToken cancellationToken) =>
        payment.Status == PaymentStatus.Refunded
            ? _orderPaymentSyncService.MarkRefundedAsync(payment.StoreId, payment.OrderId,
                payment.ExternalPaymentReference, cancellationToken)
            : Task.CompletedTask;
}
