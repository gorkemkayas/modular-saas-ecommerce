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
    private readonly ILogger<MarkShipmentDeliveryExceptionCommandHandler> _logger;

    public MarkShipmentDeliveryExceptionCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        ILogger<MarkShipmentDeliveryExceptionCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
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
        _logger.LogInformation("Shipment delivery exception recorded | ShipmentId: {ShipmentId} | OrderId: {OrderId}",
            shipment.Id, shipment.OrderId);
    }
}
