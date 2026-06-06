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
    private readonly IOrderPaymentContextService _orderPaymentContextService;
    private readonly IOrderPaymentSyncService _orderPaymentSyncService;
    private readonly IPaymentNotificationService _paymentNotificationService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IOrderPaymentContextService orderPaymentContextService,
        IOrderPaymentSyncService orderPaymentSyncService,
        IPaymentNotificationService paymentNotificationService,
        IPaymentGateway paymentGateway,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _orderPaymentContextService = orderPaymentContextService;
        _orderPaymentSyncService = orderPaymentSyncService;
        _paymentNotificationService = paymentNotificationService;
        _paymentGateway = paymentGateway;
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

        var payment = await _paymentRepository.GetByIdAsync(command.StoreId, command.PaymentId, cancellationToken)
            ?? throw new PaymentNotFoundException(command.PaymentId);

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
            payment.MarkFailed(
                command.IdempotencyKey,
                PaymentOperationType.Refund,
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

            await _orderPaymentSyncService.MarkRefundedAsync(
                payment.StoreId,
                payment.OrderId,
                payment.ExternalPaymentReference,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (gatewayResult.Outcome == PaymentGatewayOutcome.Refunded)
        {
            try
            {
                var orderContext = await _orderPaymentContextService.GetStoreOrderContextAsync(
                    payment.StoreId,
                    payment.OrderId,
                    cancellationToken);

                if (orderContext is null)
                {
                    _logger.LogWarning(
                        "Payment refunded notification skipped because order context was missing | PaymentId: {PaymentId} | OrderId: {OrderId}",
                        payment.Id,
                        payment.OrderId);
                }
                else
                {
                    await _paymentNotificationService.SendPaymentRefundedAsync(
                        payment.StoreId,
                        payment.Id,
                        payment.OrderId,
                        payment.CustomerId,
                        orderContext.OrderNumber,
                        orderContext.Customer.Email,
                        orderContext.Customer.FullName,
                        command.Amount,
                        payment.CurrencyCode,
                        cancellationToken);
                }
            }
            catch (Exception notificationException)
            {
                _logger.LogWarning(
                    notificationException,
                    "Payment refunded notification failed | PaymentId: {PaymentId} | OrderId: {OrderId}",
                    payment.Id,
                    payment.OrderId);
            }
        }

        _logger.LogInformation(
            "Payment refund handled | PaymentId: {PaymentId} | Status: {Status} | RefundedAmount: {RefundedAmount}",
            payment.Id,
            payment.Status,
            payment.RefundedAmount);

        return new RefundPaymentResult(payment.Id, payment.Status, payment.RefundedAmount);
    }
}
