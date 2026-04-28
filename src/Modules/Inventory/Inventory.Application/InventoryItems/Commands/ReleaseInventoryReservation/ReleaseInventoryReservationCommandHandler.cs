using Inventory.Application.Abstractions;
using Inventory.Application.Exceptions;
using Inventory.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.InventoryItems.Commands.ReleaseInventoryReservation;

public sealed class ReleaseInventoryReservationCommandHandler : IRequestHandler<ReleaseInventoryReservationCommand>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReleaseInventoryReservationCommandHandler> _logger;

    public ReleaseInventoryReservationCommandHandler(
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReleaseInventoryReservationCommandHandler> logger)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ReleaseInventoryReservationCommand command, CancellationToken cancellationToken)
    {
        var inventoryItems = await _inventoryItemRepository.ListByReservationReferenceAsync(
            command.StoreId,
            command.ReservationReference,
            cancellationToken);

        if (inventoryItems.Count == 0)
            throw new InventoryReservationNotFoundException(command.ReservationReference);

        foreach (var inventoryItem in inventoryItems)
            inventoryItem.ReleaseReservation(command.ReservationReference, command.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inventory reservation released | StoreId: {StoreId} | ReservationReference: {ReservationReference}",
            command.StoreId,
            command.ReservationReference);
    }
}
