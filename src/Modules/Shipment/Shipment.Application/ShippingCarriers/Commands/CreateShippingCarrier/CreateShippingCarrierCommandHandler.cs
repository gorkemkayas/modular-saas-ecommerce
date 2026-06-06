using MediatR;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Domain.Entities;
using Shipment.Domain.Repositories;

namespace Shipment.Application.ShippingCarriers.Commands.CreateShippingCarrier;

public sealed class CreateShippingCarrierCommandHandler : IRequestHandler<CreateShippingCarrierCommand, Guid>
{
    private readonly IShippingCarrierRepository _shippingCarrierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateShippingCarrierCommandHandler(
        IShippingCarrierRepository shippingCarrierRepository,
        IUnitOfWork unitOfWork)
    {
        _shippingCarrierRepository = shippingCarrierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateShippingCarrierCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty)
            throw new ShipmentValidationException("StoreId is required.");

        var normalizedCode = ShippingCarrier.NormalizeCode(command.Code);

        if (await _shippingCarrierRepository.ExistsByCodeAsync(command.StoreId, normalizedCode, null, cancellationToken))
            throw new DuplicateShippingCarrierCodeException(normalizedCode);

        var carrier = ShippingCarrier.Create(
            command.StoreId,
            normalizedCode,
            command.Name,
            command.ServiceCode,
            command.ServiceName,
            command.TrackingUrl,
            command.SortOrder);

        await _shippingCarrierRepository.AddAsync(carrier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return carrier.Id;
    }
}
