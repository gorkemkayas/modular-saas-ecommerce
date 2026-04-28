using Inventory.Application.Abstractions;
using Inventory.Application.Exceptions;
using Inventory.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.InventoryItems.Commands.AdjustStock;

public sealed class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdjustStockCommandHandler> _logger;

    public AdjustStockCommandHandler(
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<AdjustStockCommandHandler> logger)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AdjustStockCommand command, CancellationToken cancellationToken)
    {
        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(command.StoreId, command.InventoryItemId, cancellationToken)
            ?? throw new InventoryItemNotFoundException(command.InventoryItemId);

        inventoryItem.AdjustStock(command.NewOnHandQuantity, command.Reason, command.Reference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inventory stock adjusted | InventoryItemId: {InventoryItemId} | NewOnHandQuantity: {NewOnHandQuantity}",
            inventoryItem.Id,
            command.NewOnHandQuantity);
    }
}
