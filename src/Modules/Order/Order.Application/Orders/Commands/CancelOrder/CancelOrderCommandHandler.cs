using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Application.Exceptions;
using Order.Application.Integrations;
using Order.Domain.Repositories;

namespace Order.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderCustomerContextService _customerContextService;
    private readonly IOrderInventoryService _inventoryService;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IOrderCustomerContextService customerContextService,
        IOrderInventoryService inventoryService,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _customerContextService = customerContextService;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty || command.OrderId == Guid.Empty)
            throw new OrderValidationException("StoreId and OrderId are required.");

        var customerIdentity = await _customerContextService.GetCustomerIdentityAsync(
            command.StoreId,
            command.ExternalUserId,
            cancellationToken);

        if (customerIdentity is null)
            throw new UnauthorizedOrderAccessException();

        var order = await _orderRepository.GetByIdAsync(command.StoreId, command.OrderId, cancellationToken)
            ?? throw new OrderNotFoundException(command.OrderId);

        if (order.CustomerId != customerIdentity.CustomerId)
            throw new UnauthorizedOrderAccessException();

        order.Cancel(command.Reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(order.ReservationReference))
        {
            await _inventoryService.ReleaseReservationAsync(
                command.StoreId,
                order.ReservationReference,
                command.Reason ?? "Order cancelled by customer.",
                cancellationToken);
        }

        _logger.LogInformation(
            "Order cancelled | OrderId: {OrderId} | OrderNumber: {OrderNumber} | StoreId: {StoreId} | CustomerId: {CustomerId}",
            order.Id,
            order.OrderNumber.Value,
            order.StoreId,
            order.CustomerId);
    }
}
