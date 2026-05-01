using MediatR;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Domain.Repositories;

namespace Shipment.Application.Shipments.Commands.RegisterShipmentTrackingEvent;

public sealed class RegisterShipmentTrackingEventCommandHandler : IRequestHandler<RegisterShipmentTrackingEventCommand>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterShipmentTrackingEventCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RegisterShipmentTrackingEventCommand command, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(command.StoreId, command.ShipmentId, cancellationToken)
            ?? throw new ShipmentNotFoundException(command.ShipmentId);

        shipment.RegisterTrackingEvent(
            command.PackageId,
            command.Type,
            command.OccurredAtUtc,
            command.Location,
            command.Description,
            command.RawStatusCode,
            command.RawStatusText);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
