using MediatR;
using Microsoft.Extensions.Logging;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Domain.Repositories;

namespace Shipment.Application.Shipments.Commands.AddShipmentPackage;

public sealed class AddShipmentPackageCommandHandler : IRequestHandler<AddShipmentPackageCommand, Guid>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddShipmentPackageCommandHandler> _logger;

    public AddShipmentPackageCommandHandler(
        IShipmentRepository shipmentRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddShipmentPackageCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddShipmentPackageCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty || command.ShipmentId == Guid.Empty)
            throw new ShipmentValidationException("StoreId and ShipmentId are required.");

        var shipment = await _shipmentRepository.GetByIdAsync(command.StoreId, command.ShipmentId, cancellationToken)
            ?? throw new ShipmentNotFoundException(command.ShipmentId);

        var packageNumber = $"PKG-{shipment.Packages.Count + 1:D2}";
        var packageId = shipment.AddPackage(
            packageNumber,
            command.TrackingNumber,
            command.Weight,
            command.WeightUnit,
            command.LabelReference);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Shipment package added | ShipmentId: {ShipmentId} | PackageId: {PackageId} | PackageNumber: {PackageNumber}",
            shipment.Id,
            packageId,
            packageNumber);

        return packageId;
    }
}
