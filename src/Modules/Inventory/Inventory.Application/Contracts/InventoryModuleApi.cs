using Inventory.Application.Abstractions;
using Inventory.Application.Exceptions;
using Inventory.Contracts;
using Inventory.Domain.Exceptions;
using Inventory.Domain.Repositories;

namespace Inventory.Application.Contracts;

public sealed class InventoryModuleApi : IInventoryModuleApi
{
    private readonly IInventoryItemRepository _inventoryItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryModuleApi(
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork)
    {
        _inventoryItemRepository = inventoryItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InventoryAvailabilityResult> CheckAvailabilityAsync(
        CheckInventoryAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var items = new List<InventoryAvailabilityItemResult>(request.Items.Count);

        foreach (var item in request.Items)
        {
            var inventoryItem = await _inventoryItemRepository.GetBySellableItemAsync(
                request.StoreId,
                item.ProductId,
                item.ProductVariantId,
                cancellationToken);

            var availableQuantity = inventoryItem?.AvailableQuantity ?? 0;

            items.Add(new InventoryAvailabilityItemResult(
                item.ProductId,
                item.ProductVariantId,
                availableQuantity >= item.Quantity,
                item.Quantity,
                availableQuantity));
        }

        return new InventoryAvailabilityResult(items.All(x => x.IsAvailable), items);
    }

    public async Task<InventoryReservationResult> ReserveAsync(
        ReserveInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var results = new List<InventoryReservationItemResult>(request.Items.Count);

        foreach (var item in request.Items)
        {
            var inventoryItem = await _inventoryItemRepository.GetBySellableItemAsync(
                request.StoreId,
                item.ProductId,
                item.ProductVariantId,
                cancellationToken);

            if (inventoryItem is null)
                throw new InventoryInsufficientStockException(item.ProductId, item.ProductVariantId, item.Quantity);

            try
            {
                inventoryItem.Reserve(request.OrderId, request.ReservationReference, item.Quantity);
            }
            catch (InventoryDomainException ex) when (ex.Message.Contains("Insufficient available stock", StringComparison.Ordinal))
            {
                throw new InventoryInsufficientStockException(item.ProductId, item.ProductVariantId, item.Quantity);
            }

            results.Add(new InventoryReservationItemResult(
                inventoryItem.Id,
                inventoryItem.ProductId,
                inventoryItem.ProductVariantId,
                item.Quantity));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InventoryReservationResult(request.ReservationReference, results);
    }

    public async Task ReleaseReservationAsync(
        ReleaseInventoryReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        var inventoryItems = await _inventoryItemRepository.ListByReservationReferenceAsync(
            request.StoreId,
            request.ReservationReference,
            cancellationToken);

        if (inventoryItems.Count == 0)
            throw new InventoryReservationNotFoundException(request.ReservationReference);

        foreach (var inventoryItem in inventoryItems)
            inventoryItem.ReleaseReservation(request.ReservationReference, request.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfirmDeductionAsync(
        ConfirmInventoryDeductionRequest request,
        CancellationToken cancellationToken = default)
    {
        var inventoryItems = await _inventoryItemRepository.ListByReservationReferenceAsync(
            request.StoreId,
            request.ReservationReference,
            cancellationToken);

        if (inventoryItems.Count == 0)
            throw new InventoryReservationNotFoundException(request.ReservationReference);

        foreach (var inventoryItem in inventoryItems)
            inventoryItem.ConfirmReservation(request.ReservationReference, request.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
