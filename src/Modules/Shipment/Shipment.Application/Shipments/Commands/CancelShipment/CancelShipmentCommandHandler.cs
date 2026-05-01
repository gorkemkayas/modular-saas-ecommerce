using MediatR;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Application.Integrations;
using Shipment.Domain.Repositories;

namespace Shipment.Application.Shipments.Commands.CancelShipment;

public sealed class CancelShipmentCommandHandler : IRequestHandler<CancelShipmentCommand>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderShipmentSyncService _orderShipmentSyncService;

    public CancelShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        IOrderShipmentSyncService orderShipmentSyncService)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _orderShipmentSyncService = orderShipmentSyncService;
    }

    public async Task Handle(CancelShipmentCommand command, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(command.StoreId, command.ShipmentId, cancellationToken)
            ?? throw new ShipmentNotFoundException(command.ShipmentId);

        shipment.Cancel(command.Reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _orderShipmentSyncService.MarkShipmentCancelledAsync(
            shipment.StoreId,
            shipment.OrderId,
            shipment.ShipmentNumber,
            cancellationToken);
    }
}
