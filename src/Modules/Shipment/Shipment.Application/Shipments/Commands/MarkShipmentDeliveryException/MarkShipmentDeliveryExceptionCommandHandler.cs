using MediatR;
using Microsoft.Extensions.Logging;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Application.Integrations;
using Shipment.Domain.Repositories;

namespace Shipment.Application.Shipments.Commands.MarkShipmentDeliveryException;

public sealed class MarkShipmentDeliveryExceptionCommandHandler : IRequestHandler<MarkShipmentDeliveryExceptionCommand>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderShipmentContextService _orderShipmentContextService;
    private readonly IShipmentNotificationService _shipmentNotificationService;
    private readonly ILogger<MarkShipmentDeliveryExceptionCommandHandler> _logger;

    public MarkShipmentDeliveryExceptionCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        IOrderShipmentContextService orderShipmentContextService,
        IShipmentNotificationService shipmentNotificationService,
        ILogger<MarkShipmentDeliveryExceptionCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _orderShipmentContextService = orderShipmentContextService;
        _shipmentNotificationService = shipmentNotificationService;
        _logger = logger;
    }

    public async Task Handle(MarkShipmentDeliveryExceptionCommand command, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(command.StoreId, command.ShipmentId, cancellationToken)
            ?? throw new ShipmentNotFoundException(command.ShipmentId);

        shipment.MarkDeliveryException(
            command.PackageId,
            DateTime.UtcNow,
            command.Description,
            command.Location,
            command.RawStatusCode,
            command.RawStatusText);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var orderContext = await _orderShipmentContextService.GetStoreOrderContextAsync(
                shipment.StoreId,
                shipment.OrderId,
                cancellationToken);

            if (orderContext is null)
            {
                _logger.LogWarning(
                    "Shipment exception notification skipped because order context was missing | ShipmentId: {ShipmentId} | OrderId: {OrderId}",
                    shipment.Id,
                    shipment.OrderId);
                return;
            }

            await _shipmentNotificationService.SendShipmentDeliveryExceptionAsync(
                shipment.StoreId,
                shipment.Id,
                shipment.OrderId,
                orderContext.CustomerId,
                shipment.OrderNumber,
                shipment.ShipmentNumber,
                orderContext.CustomerEmail,
                orderContext.CustomerFullName,
                shipment.CarrierName,
                shipment.Packages.OrderBy(x => x.CreatedAtUtc).Select(x => x.TrackingNumber).FirstOrDefault(x => x != null),
                shipment.TrackingUrl,
                command.Description,
                cancellationToken);
        }
        catch (Exception notificationException)
        {
            _logger.LogWarning(
                notificationException,
                "Shipment exception notification failed | ShipmentId: {ShipmentId} | OrderId: {OrderId}",
                shipment.Id,
                shipment.OrderId);
        }
    }
}
