using Inventory.Application.Abstractions;
using Inventory.Application.Exceptions;
using Inventory.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.InventoryItems.Commands.SetReorderThreshold;

public sealed class SetReorderThresholdCommandHandler : IRequestHandler<SetReorderThresholdCommand>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetReorderThresholdCommandHandler> _logger;

    public SetReorderThresholdCommandHandler(
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<SetReorderThresholdCommandHandler> logger)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(SetReorderThresholdCommand command, CancellationToken cancellationToken)
    {
        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(command.StoreId, command.InventoryItemId, cancellationToken)
            ?? throw new InventoryItemNotFoundException(command.InventoryItemId);

        inventoryItem.SetReorderThreshold(command.ReorderThreshold);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inventory reorder threshold updated | InventoryItemId: {InventoryItemId} | ReorderThreshold: {ReorderThreshold}",
            inventoryItem.Id,
            command.ReorderThreshold);
    }
}
