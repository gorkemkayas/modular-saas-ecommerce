using MediatR;
using Microsoft.Extensions.Logging;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Application.Integrations;
using Shipment.Domain.Repositories;

namespace Shipment.Application.Shipments.Commands.MarkShipmentDelivered;

public sealed class MarkShipmentDeliveredCommandHandler : IRequestHandler<MarkShipmentDeliveredCommand>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderShipmentContextService _orderShipmentContextService;
    private readonly IOrderShipmentSyncService _orderShipmentSyncService;
    private readonly IShipmentNotificationService _shipmentNotificationService;
    private readonly ILogger<MarkShipmentDeliveredCommandHandler> _logger;

    public MarkShipmentDeliveredCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        IOrderShipmentContextService orderShipmentContextService,
        IOrderShipmentSyncService orderShipmentSyncService,
        IShipmentNotificationService shipmentNotificationService,
        ILogger<MarkShipmentDeliveredCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _orderShipmentContextService = orderShipmentContextService;
        _orderShipmentSyncService = orderShipmentSyncService;
        _shipmentNotificationService = shipmentNotificationService;
        _logger = logger;
    }

    public async Task Handle(MarkShipmentDeliveredCommand command, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(command.StoreId, command.ShipmentId, cancellationToken)
            ?? throw new ShipmentNotFoundException(command.ShipmentId);

        shipment.MarkDelivered();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _orderShipmentSyncService.MarkDeliveredAsync(
            shipment.StoreId,
            shipment.OrderId,
            shipment.ShipmentNumber,
            cancellationToken);

        try
        {
            var orderContext = await _orderShipmentContextService.GetStoreOrderContextAsync(
                shipment.StoreId,
                shipment.OrderId,
                cancellationToken);

            if (orderContext is null)
            {
                _logger.LogWarning(
                    "Shipment delivered notification skipped because order context was missing | ShipmentId: {ShipmentId} | OrderId: {OrderId}",
                    shipment.Id,
                    shipment.OrderId);
            }
            else
            {
                await _shipmentNotificationService.SendShipmentDeliveredAsync(
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
                    cancellationToken);
            }
        }
        catch (Exception notificationException)
        {
            _logger.LogWarning(
                notificationException,
                "Shipment delivered notification failed | ShipmentId: {ShipmentId} | OrderId: {OrderId}",
                shipment.Id,
                shipment.OrderId);
        }

        _logger.LogInformation(
            "Shipment marked delivered | ShipmentId: {ShipmentId} | ShipmentNumber: {ShipmentNumber}",
            shipment.Id,
            shipment.ShipmentNumber);
    }
}
