using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Application.Integrations;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.Payments.Commands.ProcessPaymentWebhook;

public sealed class ProcessPaymentWebhookCommandHandler : IRequestHandler<ProcessPaymentWebhookCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderPaymentContextService _orderPaymentContextService;
    private readonly IOrderPaymentSyncService _orderPaymentSyncService;
    private readonly IInventoryPaymentService _inventoryPaymentService;
    private readonly IShipmentPaymentService _shipmentPaymentService;
    private readonly IPaymentWebhookParser _paymentWebhookParser;
    private readonly ILogger<ProcessPaymentWebhookCommandHandler> _logger;

    public ProcessPaymentWebhookCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IOrderPaymentContextService orderPaymentContextService,
        IOrderPaymentSyncService orderPaymentSyncService,
        IInventoryPaymentService inventoryPaymentService,
        IShipmentPaymentService shipmentPaymentService,
        IPaymentWebhookParser paymentWebhookParser,
        ILogger<ProcessPaymentWebhookCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _orderPaymentContextService = orderPaymentContextService;
        _orderPaymentSyncService = orderPaymentSyncService;
        _inventoryPaymentService = inventoryPaymentService;
        _shipmentPaymentService = shipmentPaymentService;
        _paymentWebhookParser = paymentWebhookParser;
        _logger = logger;
    }

    public async Task Handle(ProcessPaymentWebhookCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Payload))
            throw new PaymentWebhookValidationException("Webhook payload is required.");

        var webhook = await _paymentWebhookParser.ParseAsync(
            command.Provider,
            command.Payload,
            command.Signature,
            cancellationToken);

        var payment = await _paymentRepository.GetByProviderReferenceAsync(
            webhook.Provider,
            webhook.ExternalConversationId,
            webhook.ExternalPaymentReference,
            cancellationToken);

        if (payment is null)
        {
            _logger.LogWarning(
                "Payment webhook ignored because payment could not be matched | Provider: {Provider} | ConversationId: {ConversationId} | PaymentReference: {PaymentReference}",
                webhook.Provider,
                webhook.ExternalConversationId,
                webhook.ExternalPaymentReference);
            return;
        }

        switch (webhook.Outcome)
        {
            case PaymentGatewayOutcome.RequiresAction:
                payment.MarkRequiresAction(
                    webhook.IdempotencyKey,
                    PaymentOperationType.Webhook,
                    webhook.ExternalConversationId,
                    webhook.ExternalPaymentReference);
                break;

            case PaymentGatewayOutcome.Authorized:
                payment.MarkAuthorized(
                    webhook.IdempotencyKey,
                    PaymentOperationType.Webhook,
                    webhook.ExternalConversationId,
                    webhook.ExternalPaymentReference);

                await _orderPaymentSyncService.MarkAuthorizedAsync(
                    payment.StoreId,
                    payment.OrderId,
                    payment.ExternalPaymentReference,
                    cancellationToken);
                break;

            case PaymentGatewayOutcome.Captured:
                payment.MarkCaptured(
                    webhook.IdempotencyKey,
                    webhook.ExternalConversationId,
                    webhook.ExternalPaymentReference);

                await _orderPaymentSyncService.MarkCapturedAsync(
                    payment.StoreId,
                    payment.OrderId,
                    payment.ExternalPaymentReference,
                    cancellationToken);

                var capturedOrderContext = await _orderPaymentContextService.GetStoreOrderContextAsync(
                    payment.StoreId,
                    payment.OrderId,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(capturedOrderContext?.ReservationReference))
                {
                    await _inventoryPaymentService.ConfirmDeductionAsync(
                        payment.StoreId,
                        capturedOrderContext.ReservationReference,
                        "Payment captured via webhook.",
                        cancellationToken);
                }
                break;

            case PaymentGatewayOutcome.Cancelled:
                payment.Cancel(
                    webhook.IdempotencyKey,
                    webhook.ExternalPaymentReference);

                await _orderPaymentSyncService.MarkFailedAsync(
                    payment.StoreId,
                    payment.OrderId,
                    payment.ExternalPaymentReference,
                    cancellationToken);

                var cancelledOrderContext = await _orderPaymentContextService.GetStoreOrderContextAsync(
                    payment.StoreId,
                    payment.OrderId,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(cancelledOrderContext?.ReservationReference))
                {
                    await _inventoryPaymentService.ReleaseReservationAsync(
                        payment.StoreId,
                        cancelledOrderContext.ReservationReference,
                        "Payment cancelled via webhook.",
                        cancellationToken);
                }
                break;

            case PaymentGatewayOutcome.Refunded:
                payment.Refund(
                    webhook.IdempotencyKey,
                    webhook.RefundAmount ?? payment.Amount,
                    "Payment refunded via webhook.",
                    webhook.ExternalPaymentReference);

                await _orderPaymentSyncService.MarkRefundedAsync(
                    payment.StoreId,
                    payment.OrderId,
                    payment.ExternalPaymentReference,
                    cancellationToken);
                break;

            case PaymentGatewayOutcome.Failed:
                payment.MarkFailed(
                    webhook.IdempotencyKey,
                    PaymentOperationType.Webhook,
                    webhook.FailureCode,
                    webhook.FailureMessage,
                    providerTransactionReference: webhook.ExternalPaymentReference);

                await _orderPaymentSyncService.MarkFailedAsync(
                    payment.StoreId,
                    payment.OrderId,
                    payment.ExternalPaymentReference,
                    cancellationToken);
                break;

            default:
                throw new PaymentWebhookValidationException("Unsupported webhook outcome.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (webhook.Outcome == PaymentGatewayOutcome.Captured)
        {
            await _shipmentPaymentService.EnsureShipmentCreatedForCapturedOrderAsync(
                payment.StoreId,
                payment.OrderId,
                cancellationToken);
        }
    }
}
