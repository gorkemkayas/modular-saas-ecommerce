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
    private readonly IOrderShipmentSyncService _orderShipmentSyncService;
    private readonly ILogger<MarkShipmentDeliveredCommandHandler> _logger;

    public MarkShipmentDeliveredCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        IOrderShipmentSyncService orderShipmentSyncService,
        ILogger<MarkShipmentDeliveredCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _orderShipmentSyncService = orderShipmentSyncService;
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

        _logger.LogInformation(
            "Shipment marked delivered | ShipmentId: {ShipmentId} | ShipmentNumber: {ShipmentNumber}",
            shipment.Id,
            shipment.ShipmentNumber);
    }
}
