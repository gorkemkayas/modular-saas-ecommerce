using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Application.Integrations;
using Payment.Domain.Repositories;

namespace Payment.Application.Payments.Commands.CancelPayment;

public sealed class CancelPaymentCommandHandler : IRequestHandler<CancelPaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderPaymentContextService _orderPaymentContextService;
    private readonly IOrderPaymentSyncService _orderPaymentSyncService;
    private readonly IInventoryPaymentService _inventoryPaymentService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<CancelPaymentCommandHandler> _logger;

    public CancelPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IOrderPaymentContextService orderPaymentContextService,
        IOrderPaymentSyncService orderPaymentSyncService,
        IInventoryPaymentService inventoryPaymentService,
        IPaymentGateway paymentGateway,
        ILogger<CancelPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _orderPaymentContextService = orderPaymentContextService;
        _orderPaymentSyncService = orderPaymentSyncService;
        _inventoryPaymentService = inventoryPaymentService;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task Handle(CancelPaymentCommand command, CancellationToken cancellationToken)
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

        var gatewayResult = await _paymentGateway.CancelAsync(
            new PaymentGatewayCancelRequest(
                payment.Id,
                payment.StoreId,
                payment.OrderId,
                payment.Amount,
                payment.CurrencyCode,
                payment.ExternalPaymentReference,
                payment.ExternalConversationId,
                command.IdempotencyKey),
            cancellationToken);

        if (gatewayResult.Outcome != PaymentGatewayOutcome.Cancelled)
            throw new PaymentValidationException(gatewayResult.FailureMessage ?? "Payment could not be cancelled.");

        payment.Cancel(
            command.IdempotencyKey,
            gatewayResult.ExternalPaymentReference,
            gatewayResult.ProviderRequestReference);

        await _orderPaymentSyncService.MarkFailedAsync(
            payment.StoreId,
            payment.OrderId,
            payment.ExternalPaymentReference,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(orderContext?.ReservationReference))
        {
            await _inventoryPaymentService.ReleaseReservationAsync(
                payment.StoreId,
                orderContext.ReservationReference,
                "Payment cancelled.",
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment cancelled | PaymentId: {PaymentId} | OrderId: {OrderId}",
            payment.Id,
            payment.OrderId);
    }
}
