using Inventory.Application.Abstractions;
using Inventory.Application.Exceptions;
using Inventory.Application.Integrations;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.InventoryItems.Commands.CreateInventoryItem;

public sealed class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, Guid>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IInventoryCatalogService _catalogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateInventoryItemCommandHandler> _logger;

    public CreateInventoryItemCommandHandler(
        IInventoryItemRepository inventoryItemRepository,
        IInventoryCatalogService catalogService,
        IUnitOfWork unitOfWork,
        ILogger<CreateInventoryItemCommandHandler> logger)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _catalogService = catalogService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateInventoryItemCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty)
            throw new InventoryValidationException("StoreId is required.");

        if (command.ProductId == Guid.Empty)
            throw new InventoryValidationException("ProductId is required.");

        if (command.InitialOnHandQuantity < 0)
            throw new InventoryValidationException("InitialOnHandQuantity cannot be negative.");

        if (command.ReorderThreshold.HasValue && command.ReorderThreshold.Value < 0)
            throw new InventoryValidationException("ReorderThreshold cannot be negative.");

        if (await _inventoryItemRepository.ExistsBySellableItemAsync(
                command.StoreId,
                command.ProductId,
                command.ProductVariantId,
                cancellationToken: cancellationToken))
        {
            throw new DuplicateInventoryItemException(command.StoreId, command.ProductId, command.ProductVariantId);
        }

        var sellableItem = await _catalogService.GetSellableItemAsync(
            command.StoreId,
            command.ProductId,
            command.ProductVariantId,
            cancellationToken);

        if (sellableItem is null)
            throw new InventoryValidationException("Sellable catalog item was not found or is not active.");

        var displayName = string.IsNullOrWhiteSpace(sellableItem.VariantName)
            ? sellableItem.ProductName
            : $"{sellableItem.ProductName} / {sellableItem.VariantName}";

        var inventoryItem = InventoryItem.Create(
            command.StoreId,
            command.ProductId,
            command.ProductVariantId,
            sellableItem.Sku,
            displayName,
            command.InitialOnHandQuantity,
            command.ReorderThreshold);

        await _inventoryItemRepository.AddAsync(inventoryItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Inventory item created | InventoryItemId: {InventoryItemId} | StoreId: {StoreId} | ProductId: {ProductId} | VariantId: {VariantId}",
            inventoryItem.Id,
            command.StoreId,
            command.ProductId,
            command.ProductVariantId);

        return inventoryItem.Id;
    }
}
