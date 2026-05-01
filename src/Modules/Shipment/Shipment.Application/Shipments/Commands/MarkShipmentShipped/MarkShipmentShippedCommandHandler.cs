using MediatR;
using Microsoft.Extensions.Logging;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Application.Integrations;
using Shipment.Domain.Repositories;

namespace Shipment.Application.Shipments.Commands.MarkShipmentShipped;

public sealed class MarkShipmentShippedCommandHandler : IRequestHandler<MarkShipmentShippedCommand>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderShipmentSyncService _orderShipmentSyncService;
    private readonly ILogger<MarkShipmentShippedCommandHandler> _logger;

    public MarkShipmentShippedCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        IOrderShipmentSyncService orderShipmentSyncService,
        ILogger<MarkShipmentShippedCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _orderShipmentSyncService = orderShipmentSyncService;
        _logger = logger;
    }

    public async Task Handle(MarkShipmentShippedCommand command, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(command.StoreId, command.ShipmentId, cancellationToken)
            ?? throw new ShipmentNotFoundException(command.ShipmentId);

        shipment.MarkShipped();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _orderShipmentSyncService.MarkShippedAsync(
            shipment.StoreId,
            shipment.OrderId,
            shipment.ShipmentNumber,
            cancellationToken);

        _logger.LogInformation(
            "Shipment marked shipped | ShipmentId: {ShipmentId} | ShipmentNumber: {ShipmentNumber}",
            shipment.Id,
            shipment.ShipmentNumber);
    }
}
