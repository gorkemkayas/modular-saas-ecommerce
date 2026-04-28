using Inventory.Application.Abstractions;
using Inventory.Application.Exceptions;
using Inventory.Domain.Exceptions;
using Inventory.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.InventoryItems.Commands.ReserveInventory;

public sealed class ReserveInventoryCommandHandler : IRequestHandler<ReserveInventoryCommand>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReserveInventoryCommandHandler> _logger;

    public ReserveInventoryCommandHandler(
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReserveInventoryCommandHandler> logger)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ReserveInventoryCommand command, CancellationToken cancellationToken)
    {
        foreach (var item in command.Items)
        {
            var inventoryItem = await _inventoryItemRepository.GetBySellableItemAsync(
                command.StoreId,
                item.ProductId,
                item.ProductVariantId,
                cancellationToken);

            if (inventoryItem is null)
                throw new InventoryInsufficientStockException(item.ProductId, item.ProductVariantId, item.Quantity);

            try
            {
                inventoryItem.Reserve(command.OrderId, command.ReservationReference, item.Quantity);
            }
            catch (InventoryDomainException ex) when (ex.Message.Contains("Insufficient available stock", StringComparison.Ordinal))
            {
                throw new InventoryInsufficientStockException(item.ProductId, item.ProductVariantId, item.Quantity);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inventory reserved | StoreId: {StoreId} | OrderId: {OrderId} | ReservationReference: {ReservationReference}",
            command.StoreId,
            command.OrderId,
            command.ReservationReference);
    }
}
