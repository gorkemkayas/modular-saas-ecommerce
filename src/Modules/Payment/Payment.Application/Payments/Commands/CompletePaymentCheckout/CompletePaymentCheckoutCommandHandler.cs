using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Application.Integrations;
using Payment.Application.Payments.DTOs;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.Payments.Commands.CompletePaymentCheckout;

public sealed class CompletePaymentCheckoutCommandHandler : IRequestHandler<CompletePaymentCheckoutCommand, PaymentActionResultDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderPaymentContextService _orderPaymentContextService;
    private readonly IOrderPaymentSyncService _orderPaymentSyncService;
    private readonly IInventoryPaymentService _inventoryPaymentService;
    private readonly IShipmentPaymentService _shipmentPaymentService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<CompletePaymentCheckoutCommandHandler> _logger;

    public CompletePaymentCheckoutCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IOrderPaymentContextService orderPaymentContextService,
        IOrderPaymentSyncService orderPaymentSyncService,
        IInventoryPaymentService inventoryPaymentService,
        IShipmentPaymentService shipmentPaymentService,
        IPaymentGateway paymentGateway,
        ILogger<CompletePaymentCheckoutCommandHandler> logger)
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

    public async Task<PaymentActionResultDto> Handle(CompletePaymentCheckoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new PaymentValidationException("Checkout token is required.");

        if (_paymentGateway.Provider != command.Provider)
            throw new PaymentValidationException("Configured payment gateway does not match callback provider.");

        var gatewayResult = await _paymentGateway.CompleteAsync(
            new PaymentGatewayCompleteRequest(command.Token),
            cancellationToken);

        var payment = await _paymentRepository.GetByProviderReferenceAsync(
            command.Provider,
            gatewayResult.ExternalConversationId,
            gatewayResult.ExternalPaymentReference,
            cancellationToken);

        if (payment is null)
            throw new PaymentValidationException("Payment could not be matched from checkout callback.");

        switch (gatewayResult.Outcome)
        {
            case PaymentGatewayOutcome.Authorized:
                payment.MarkAuthorized(
                    command.Token,
                    PaymentOperationType.Authorize,
                    gatewayResult.ExternalConversationId,
                    gatewayResult.ExternalPaymentReference,
                    gatewayResult.ProviderRequestReference);

                await _orderPaymentSyncService.MarkAuthorizedAsync(
                    payment.StoreId,
                    payment.OrderId,
                    payment.ExternalPaymentReference,
                    cancellationToken);
                break;

            case PaymentGatewayOutcome.Captured:
                payment.MarkCaptured(
                    command.Token,
                    gatewayResult.ExternalConversationId,
                    gatewayResult.ExternalPaymentReference,
                    gatewayResult.ProviderRequestReference);

                await _orderPaymentSyncService.MarkCapturedAsync(
                    payment.StoreId,
                    payment.OrderId,
                    payment.ExternalPaymentReference,
                    cancellationToken);

                var orderContext = await _orderPaymentContextService.GetStoreOrderContextAsync(
                    payment.StoreId,
                    payment.OrderId,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(orderContext?.ReservationReference))
                {
                    await _inventoryPaymentService.ConfirmDeductionAsync(
                        payment.StoreId,
                        orderContext.ReservationReference,
                        "Payment captured from hosted checkout callback.",
                        cancellationToken);
                }
                break;

            case PaymentGatewayOutcome.Failed:
                payment.MarkFailed(
                    command.Token,
                    PaymentOperationType.Authorize,
                    gatewayResult.FailureCode,
                    gatewayResult.FailureMessage,
                    gatewayResult.ProviderRequestReference,
                    gatewayResult.ExternalPaymentReference);

                await _orderPaymentSyncService.MarkFailedAsync(
                    payment.StoreId,
                    payment.OrderId,
                    payment.ExternalPaymentReference,
                    cancellationToken);
                break;

            default:
                throw new PaymentValidationException("Unsupported checkout completion result.");
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
            "Hosted checkout completion handled | Provider: {Provider} | PaymentId: {PaymentId} | Status: {Status}",
            command.Provider,
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
