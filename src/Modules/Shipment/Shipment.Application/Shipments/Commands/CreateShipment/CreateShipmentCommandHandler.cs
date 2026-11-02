using MediatR;
using Microsoft.Extensions.Logging;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Application.Integrations;
using Shipment.Domain.Entities;
using Shipment.Domain.Repositories;
using Shipment.Domain.ValueObjects;

namespace Shipment.Application.Shipments.Commands.CreateShipment;

public sealed class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, Guid>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShipmentNumberGenerator _shipmentNumberGenerator;
    private readonly IOrderShipmentContextService _orderShipmentContextService;
    private readonly ILogger<CreateShipmentCommandHandler> _logger;

    public CreateShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        IShipmentNumberGenerator shipmentNumberGenerator,
        IOrderShipmentContextService orderShipmentContextService,
        ILogger<CreateShipmentCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _shipmentNumberGenerator = shipmentNumberGenerator;
        _orderShipmentContextService = orderShipmentContextService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty || command.OrderId == Guid.Empty)
            throw new ShipmentValidationException("StoreId and OrderId are required.");

        if (await _shipmentRepository.ExistsActiveForOrderAsync(command.StoreId, command.OrderId, cancellationToken))
            throw new ShipmentAlreadyExistsForOrderException(command.OrderId);

        var orderContext = await _orderShipmentContextService.GetStoreOrderContextAsync(
            command.StoreId,
            command.OrderId,
            cancellationToken);

        if (orderContext is null)
            throw new ShipmentCreationNotAllowedException("Order context could not be resolved for shipment creation.");

        if (orderContext.Status == OrderShipmentStatus.Cancelled)
            throw new ShipmentCreationNotAllowedException("Cancelled order cannot be shipped.");

        if (orderContext.PaymentStatus != OrderShipmentPaymentStatus.Captured)
            throw new ShipmentCreationNotAllowedException("Only captured orders can be shipped.");

        // Active shipment ownership belongs to Shipment; Order is an asynchronous projection.

        var shipment = Shipment.Domain.Entities.Shipment.Create(
            command.StoreId,
            command.OrderId,
            orderContext.OrderNumber,
            await _shipmentNumberGenerator.GenerateAsync(command.StoreId, cancellationToken),
            orderContext.ShippingAddress.ContactName,
            orderContext.ShippingAddress.PhoneNumber,
            ShipmentAddress.Create(
                orderContext.ShippingAddress.ContactName,
                orderContext.ShippingAddress.PhoneNumber,
                orderContext.ShippingAddress.Country,
                orderContext.ShippingAddress.City,
                orderContext.ShippingAddress.District,
                orderContext.ShippingAddress.Line1,
                orderContext.ShippingAddress.Line2,
                orderContext.ShippingAddress.PostalCode),
            orderContext.Items
                .Select(item => new ShipmentLineDraft(
                    item.OrderItemId,
                    item.ProductId,
                    item.ProductVariantId,
                    item.ProductName,
                    item.VariantName,
                    item.Sku,
                    item.Quantity))
                .ToArray(),
            command.InternalNote,
            orderContext.CustomerId,
            orderContext.CustomerEmail,
            orderContext.CustomerFullName);

        if (orderContext.ShippingCarrier is not null)
        {
            shipment.AssignCarrier(
                orderContext.ShippingCarrier.Code,
                orderContext.ShippingCarrier.Name,
                orderContext.ShippingCarrier.ServiceCode,
                orderContext.ShippingCarrier.ServiceName,
                orderContext.ShippingCarrier.TrackingUrl);
        }

        await _shipmentRepository.AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Shipment created | ShipmentId: {ShipmentId} | ShipmentNumber: {ShipmentNumber} | StoreId: {StoreId} | OrderId: {OrderId}",
            shipment.Id,
            shipment.ShipmentNumber,
            shipment.StoreId,
            shipment.OrderId);

        return shipment.Id;
    }
}
