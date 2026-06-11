using MediatR;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Domain.Entities;
using Shipment.Domain.Repositories;

namespace Shipment.Application.ShippingCarriers.Commands.UpdateShippingCarrier;

public sealed class UpdateShippingCarrierCommandHandler : IRequestHandler<UpdateShippingCarrierCommand>
{
    private readonly IShippingCarrierRepository _shippingCarrierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateShippingCarrierCommandHandler(
        IShippingCarrierRepository shippingCarrierRepository,
        IUnitOfWork unitOfWork)
    {
        _shippingCarrierRepository = shippingCarrierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateShippingCarrierCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty || command.CarrierId == Guid.Empty)
            throw new ShipmentValidationException("StoreId and CarrierId are required.");

        var normalizedCode = ShippingCarrier.NormalizeCode(command.Code);

        if (await _shippingCarrierRepository.ExistsByCodeAsync(command.StoreId, normalizedCode, command.CarrierId, cancellationToken))
            throw new DuplicateShippingCarrierCodeException(normalizedCode);

        var carrier = await _shippingCarrierRepository.GetByIdAsync(command.StoreId, command.CarrierId, cancellationToken)
            ?? throw new ShippingCarrierNotFoundException(command.CarrierId);

        carrier.Update(
            normalizedCode,
            command.Name,
            command.ServiceCode,
            command.ServiceName,
            command.TrackingUrl,
            command.IsActive,
            command.SortOrder);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
