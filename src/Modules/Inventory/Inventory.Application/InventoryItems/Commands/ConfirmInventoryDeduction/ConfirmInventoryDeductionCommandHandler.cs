using Inventory.Application.Abstractions;
using Inventory.Application.Exceptions;
using Inventory.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.InventoryItems.Commands.ConfirmInventoryDeduction;

public sealed class ConfirmInventoryDeductionCommandHandler : IRequestHandler<ConfirmInventoryDeductionCommand>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfirmInventoryDeductionCommandHandler> _logger;

    public ConfirmInventoryDeductionCommandHandler(
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConfirmInventoryDeductionCommandHandler> logger)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ConfirmInventoryDeductionCommand command, CancellationToken cancellationToken)
    {
        var inventoryItems = await _inventoryItemRepository.ListByReservationReferenceAsync(
            command.StoreId,
            command.ReservationReference,
            cancellationToken);

        if (inventoryItems.Count == 0)
            throw new InventoryReservationNotFoundException(command.ReservationReference);

        foreach (var inventoryItem in inventoryItems)
            inventoryItem.ConfirmReservation(command.ReservationReference, command.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inventory deduction confirmed | StoreId: {StoreId} | ReservationReference: {ReservationReference}",
            command.StoreId,
            command.ReservationReference);
    }
}
