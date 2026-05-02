using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.Exceptions;
using Payment.Application.Integrations;
using Payment.Domain.Entities;
using Payment.Domain.Repositories;

namespace Payment.Application.Payments.Commands.CreatePaymentForOrder;

public sealed class CreatePaymentForOrderCommandHandler : IRequestHandler<CreatePaymentForOrderCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderPaymentContextService _orderPaymentContextService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<CreatePaymentForOrderCommandHandler> _logger;

    public CreatePaymentForOrderCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        IOrderPaymentContextService orderPaymentContextService,
        IPaymentGateway paymentGateway,
        ILogger<CreatePaymentForOrderCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _orderPaymentContextService = orderPaymentContextService;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreatePaymentForOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty || command.OrderId == Guid.Empty || command.ExternalUserId == Guid.Empty)
            throw new PaymentValidationException("StoreId, ExternalUserId and OrderId are required.");

        var orderContext = await _orderPaymentContextService.GetCustomerOrderContextAsync(
            command.StoreId,
            command.ExternalUserId,
            command.OrderId,
            cancellationToken);

        if (orderContext is null)
            throw new UnauthorizedPaymentAccessException();

        PaymentOrderStateGuard.EnsureCanCreatePayment(orderContext);

        var existingPayment = await _paymentRepository.GetByOrderIdAsync(
            command.StoreId,
            command.OrderId,
            cancellationToken);

        if (existingPayment is not null)
            return existingPayment.Id;

        var payment = Payment.Domain.Entities.Payment.Create(
            orderContext.StoreId,
            orderContext.OrderId,
            orderContext.OrderNumber,
            orderContext.CustomerId,
            orderContext.GrandTotalAmount,
            orderContext.CurrencyCode,
            _paymentGateway.Provider,
            command.MethodType);

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment created | PaymentId: {PaymentId} | OrderId: {OrderId} | StoreId: {StoreId}",
            payment.Id,
            payment.OrderId,
            payment.StoreId);

        return payment.Id;
    }
}
