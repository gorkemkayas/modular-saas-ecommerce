using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Application.Integrations;
using Payment.Application.Payments.DTOs;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.Payments.Commands.AuthorizePayment;

public sealed class AuthorizePaymentCommandHandler : IRequestHandler<AuthorizePaymentCommand, PaymentActionResultDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderPaymentContextService _orderPaymentContextService;
    private readonly IOrderPaymentSyncService _orderPaymentSyncService;
    private readonly IInventoryPaymentService _inventoryPaymentService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<AuthorizePaymentCommandHandler> _logger;

    public AuthorizePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IOrderPaymentContextService orderPaymentContextService,
        IOrderPaymentSyncService orderPaymentSyncService,
        IInventoryPaymentService inventoryPaymentService,
        IPaymentGateway paymentGateway,
        ILogger<AuthorizePaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _orderPaymentContextService = orderPaymentContextService;
        _orderPaymentSyncService = orderPaymentSyncService;
        _inventoryPaymentService = inventoryPaymentService;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<PaymentActionResultDto> Handle(AuthorizePaymentCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty || command.OrderId == Guid.Empty || command.ExternalUserId == Guid.Empty)
            throw new PaymentValidationException("StoreId, ExternalUserId and OrderId are required.");

        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
            throw new PaymentValidationException("IdempotencyKey is required.");

        var orderContext = await _orderPaymentContextService.GetCustomerOrderContextAsync(
            command.StoreId,
            command.ExternalUserId,
            command.OrderId,
            cancellationToken);

        if (orderContext is null)
            throw new UnauthorizedPaymentAccessException();

        var payment = await _paymentRepository.GetByOrderIdAsync(command.StoreId, command.OrderId, cancellationToken)
            ?? throw new PaymentValidationException("Payment must be created before authorization.");

        var gatewayResult = await _paymentGateway.AuthorizeAsync(
            new PaymentGatewayAuthorizeRequest(
                payment.Id,
                payment.StoreId,
                payment.OrderId,
                payment.OrderNumber,
                payment.CustomerId,
                payment.Amount,
                payment.CurrencyCode,
                payment.MethodType,
                command.IdempotencyKey,
                command.ClientIpAddress ?? string.Empty,
                new PaymentGatewayCustomer(
                    orderContext.Customer.Email,
                    orderContext.Customer.FullName,
                    orderContext.Customer.PhoneNumber),
                new PaymentGatewayAddress(
                    orderContext.BillingAddress.ContactName,
                    orderContext.BillingAddress.PhoneNumber,
                    orderContext.BillingAddress.Country,
                    orderContext.BillingAddress.City,
                    orderContext.BillingAddress.District,
                    orderContext.BillingAddress.Line1,
                    orderContext.BillingAddress.Line2,
                    orderContext.BillingAddress.PostalCode),
                new PaymentGatewayAddress(
                    orderContext.ShippingAddress.ContactName,
                    orderContext.ShippingAddress.PhoneNumber,
                    orderContext.ShippingAddress.Country,
                    orderContext.ShippingAddress.City,
                    orderContext.ShippingAddress.District,
                    orderContext.ShippingAddress.Line1,
                    orderContext.ShippingAddress.Line2,
                    orderContext.ShippingAddress.PostalCode),
                orderContext.Items
                    .Select(item => new PaymentGatewayBasketItem(
                        item.ProductId,
                        item.ProductName,
                        item.VariantName,
                        item.Sku,
                        item.Quantity,
                        item.LineTotalAmount))
                    .ToArray()),
            cancellationToken);

        switch (gatewayResult.Outcome)
        {
            case PaymentGatewayOutcome.RequiresAction:
                payment.MarkRequiresAction(
                    command.IdempotencyKey,
                    PaymentOperationType.Authorize,
                    gatewayResult.ExternalConversationId,
                    gatewayResult.ExternalPaymentReference,
                    gatewayResult.ProviderRequestReference);
                break;

            case PaymentGatewayOutcome.Authorized:
                payment.MarkAuthorized(
                    command.IdempotencyKey,
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
                break;

            case PaymentGatewayOutcome.Failed:
                payment.MarkFailed(
                    command.IdempotencyKey,
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
                throw new PaymentValidationException("Unsupported authorization result.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment authorization handled | PaymentId: {PaymentId} | OrderId: {OrderId} | Status: {Status}",
            payment.Id,
            payment.OrderId,
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
