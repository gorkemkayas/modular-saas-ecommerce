using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Application.Integrations;
using Payment.Application.Payments.DTOs;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.Payments.Commands.CapturePayment;

public sealed class CapturePaymentCommandHandler : IRequestHandler<CapturePaymentCommand, PaymentActionResultDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderPaymentContextService _orderPaymentContextService;
    private readonly IOrderPaymentSyncService _orderPaymentSyncService;
    private readonly IInventoryPaymentService _inventoryPaymentService;
    private readonly IShipmentPaymentService _shipmentPaymentService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<CapturePaymentCommandHandler> _logger;

    public CapturePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IOrderPaymentContextService orderPaymentContextService,
        IOrderPaymentSyncService orderPaymentSyncService,
        IInventoryPaymentService inventoryPaymentService,
        IShipmentPaymentService shipmentPaymentService,
        IPaymentGateway paymentGateway,
        ILogger<CapturePaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _orderPaymentContextService = orderPaymentContextService;
        _orderPaymentSyncService = orderPaymentSyncService;
        _inventoryPaymentService = inventoryPaymentService;
        _shipmentPaymentService = shipmentPaymentService;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<PaymentActionResultDto> Handle(CapturePaymentCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty || command.PaymentId == Guid.Empty)
            throw new PaymentValidationException("StoreId and PaymentId are required.");

        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
            throw new PaymentValidationException("IdempotencyKey is required.");

        var payment = await _paymentRepository.GetByIdAsync(command.StoreId, command.PaymentId, cancellationToken)
            ?? throw new PaymentNotFoundException(command.PaymentId);

        var orderContext = await _orderPaymentContextService.GetStoreOrderContextAsync(
            payment.StoreId,
            payment.OrderId,
            cancellationToken);

        if (orderContext is null)
            throw new PaymentValidationException("Order context could not be resolved for payment capture.");

        var gatewayResult = await _paymentGateway.CaptureAsync(
            new PaymentGatewayCaptureRequest(
                payment.Id,
                payment.StoreId,
                payment.OrderId,
                payment.Amount,
                payment.CurrencyCode,
                payment.ExternalPaymentReference,
                payment.ExternalConversationId,
                command.IdempotencyKey),
            cancellationToken);

        if (gatewayResult.Outcome == PaymentGatewayOutcome.Captured)
        {
            payment.MarkCaptured(
                command.IdempotencyKey,
                gatewayResult.ExternalConversationId,
                gatewayResult.ExternalPaymentReference,
                gatewayResult.ProviderRequestReference);

            await _orderPaymentSyncService.MarkCapturedAsync(
                payment.StoreId,
                payment.OrderId,
                payment.ExternalPaymentReference,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(orderContext.ReservationReference))
            {
                await _inventoryPaymentService.ConfirmDeductionAsync(
                    payment.StoreId,
                    orderContext.ReservationReference,
                    "Payment captured.",
                    cancellationToken);
            }
        }
        else
        {
            payment.MarkFailed(
                command.IdempotencyKey,
                PaymentOperationType.Capture,
                gatewayResult.FailureCode,
                gatewayResult.FailureMessage,
                gatewayResult.ProviderRequestReference,
                gatewayResult.ExternalPaymentReference);

            await _orderPaymentSyncService.MarkFailedAsync(
                payment.StoreId,
                payment.OrderId,
                payment.ExternalPaymentReference,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (gatewayResult.Outcome == PaymentGatewayOutcome.Captured)
        {
            await _shipmentPaymentService.EnsureShipmentCreatedForCapturedOrderAsync(
                payment.StoreId,
                payment.OrderId,
                cancellationToken);
        }

        _logger.LogInformation(
            "Payment capture handled | PaymentId: {PaymentId} | Status: {Status}",
            payment.Id,
            payment.Status);

        return new PaymentActionResultDto(
            payment.Id,
            payment.Status,
            payment.ExternalPaymentReference,
            payment.ExternalConversationId,
            gatewayResult.ActionUrl,
            gatewayResult.FailureCode,
            gatewayResult.FailureMessage);
    }
}
