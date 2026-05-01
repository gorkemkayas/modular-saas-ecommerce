using MediatR;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Domain.Repositories;

namespace Shipment.Application.Shipments.Commands.AssignShipmentCarrier;

public sealed class AssignShipmentCarrierCommandHandler : IRequestHandler<AssignShipmentCarrierCommand>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignShipmentCarrierCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AssignShipmentCarrierCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty || command.ShipmentId == Guid.Empty)
            throw new ShipmentValidationException("StoreId and ShipmentId are required.");

        var shipment = await _shipmentRepository.GetByIdAsync(command.StoreId, command.ShipmentId, cancellationToken)
            ?? throw new ShipmentNotFoundException(command.ShipmentId);

        shipment.AssignCarrier(
            command.CarrierCode,
            command.CarrierName,
            command.ServiceCode,
            command.ServiceName,
            command.TrackingUrl);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
