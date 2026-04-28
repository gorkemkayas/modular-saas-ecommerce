using Inventory.Application.Abstractions;
using Inventory.Application.Exceptions;
using Inventory.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.InventoryItems.Commands.AddStock;

public sealed class AddStockCommandHandler : IRequestHandler<AddStockCommand>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddStockCommandHandler> _logger;

    public AddStockCommandHandler(
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddStockCommandHandler> logger)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AddStockCommand command, CancellationToken cancellationToken)
    {
        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(command.StoreId, command.InventoryItemId, cancellationToken)
            ?? throw new InventoryItemNotFoundException(command.InventoryItemId);

        inventoryItem.AddStock(command.Quantity, command.Reason, command.Reference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inventory stock added | InventoryItemId: {InventoryItemId} | Quantity: {Quantity}",
            inventoryItem.Id,
            command.Quantity);
    }
}
