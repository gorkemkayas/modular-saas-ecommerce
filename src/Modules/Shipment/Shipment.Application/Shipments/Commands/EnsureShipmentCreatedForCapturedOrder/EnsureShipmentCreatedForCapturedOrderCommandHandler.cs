using MediatR;
using Shipment.Application.Exceptions;
using Shipment.Application.Shipments.Commands.CreateShipment;
using Shipment.Domain.Repositories;

namespace Shipment.Application.Shipments.Commands.EnsureShipmentCreatedForCapturedOrder;

public sealed class EnsureShipmentCreatedForCapturedOrderCommandHandler
    : IRequestHandler<EnsureShipmentCreatedForCapturedOrderCommand, Guid>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ISender _sender;

    public EnsureShipmentCreatedForCapturedOrderCommandHandler(
        IShipmentRepository shipmentRepository,
        ISender sender)
    {
        _shipmentRepository = shipmentRepository;
        _sender = sender;
    }

    public async Task<Guid> Handle(
        EnsureShipmentCreatedForCapturedOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty || command.OrderId == Guid.Empty)
            throw new ShipmentValidationException("StoreId and OrderId are required.");

        var existingShipment = await _shipmentRepository.GetActiveForOrderAsync(
            command.StoreId,
            command.OrderId,
            cancellationToken);

        if (existingShipment is not null)
            return existingShipment.Id;

        try
        {
            return await _sender.Send(
                new CreateShipmentCommand(command.StoreId, command.OrderId, command.InternalNote),
                cancellationToken);
        }
        catch (ShipmentAlreadyExistsForOrderException)
        {
            existingShipment = await _shipmentRepository.GetActiveForOrderAsync(
                command.StoreId,
                command.OrderId,
                cancellationToken);

            if (existingShipment is not null)
                return existingShipment.Id;

            throw;
        }
    }
}
