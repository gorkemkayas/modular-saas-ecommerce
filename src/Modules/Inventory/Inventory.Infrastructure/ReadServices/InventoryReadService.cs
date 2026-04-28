using Inventory.Application.Abstractions.Queries;
using Inventory.Application.Common.Models;
using Inventory.Application.InventoryItems.DTOs;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.ReadServices;

public sealed class InventoryReadService : IInventoryReadService
{
    private readonly InventoryDbContext _context;

    public InventoryReadService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItemDto?> GetByIdAsync(Guid storeId, Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        var inventoryItem = await _context.InventoryItems
            .AsNoTracking()
            .Include(x => x.Reservations)
            .Include(x => x.Movements)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == inventoryItemId, cancellationToken);

        if (inventoryItem is null)
            return null;

        return new InventoryItemDto(
            inventoryItem.Id,
            inventoryItem.StoreId,
            inventoryItem.ProductId,
            inventoryItem.ProductVariantId,
            inventoryItem.Sku,
            inventoryItem.DisplayName,
            inventoryItem.OnHandQuantity,
            inventoryItem.ReservedQuantity,
            inventoryItem.AvailableQuantity,
            inventoryItem.ReorderThreshold,
            inventoryItem.IsLowStock,
            inventoryItem.Version,
            inventoryItem.CreatedAtUtc,
            inventoryItem.UpdatedAtUtc,
            inventoryItem.Reservations
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new InventoryReservationDto(
                    x.Id,
                    x.OrderId,
                    x.ReservationReference,
                    x.Quantity,
                    x.Status,
                    x.CreatedAtUtc,
                    x.ReleasedAtUtc,
                    x.ConfirmedAtUtc))
                .ToArray(),
            inventoryItem.Movements
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(20)
                .Select(x => new StockMovementDto(
                    x.Id,
                    x.Type,
                    x.OnHandDelta,
                    x.ReservedDelta,
                    x.ResultingOnHandQuantity,
                    x.ResultingReservedQuantity,
                    x.Reason,
                    x.Reference,
                    x.CreatedAtUtc))
                .ToArray());
    }

    public async Task<PagedResult<StockMovementDto>> GetMovementsAsync(
        Guid storeId,
        Guid inventoryItemId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.StockMovements
            .AsNoTracking()
            .Where(x => x.InventoryItemId == inventoryItemId)
            .Join(
                _context.InventoryItems.AsNoTracking().Where(x => x.StoreId == storeId),
                movement => movement.InventoryItemId,
                item => item.Id,
                (movement, _) => movement);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockMovementDto(
                x.Id,
                x.Type,
                x.OnHandDelta,
                x.ReservedDelta,
                x.ResultingOnHandQuantity,
                x.ResultingReservedQuantity,
                x.Reason,
                x.Reference,
                x.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<StockMovementDto>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<PagedResult<InventoryItemSummaryDto>> SearchAsync(
        InventoryItemSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = _context.InventoryItems
            .AsNoTracking()
            .Where(x => x.StoreId == criteria.StoreId);

        if (criteria.ProductId.HasValue)
            query = query.Where(x => x.ProductId == criteria.ProductId.Value);

        if (criteria.ProductVariantId.HasValue)
            query = query.Where(x => x.ProductVariantId == criteria.ProductVariantId.Value);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.Trim();
            query = query.Where(x => x.Sku.Contains(searchTerm) || x.DisplayName.Contains(searchTerm));
        }

        if (criteria.OnlyLowStock)
            query = query.Where(x => x.ReorderThreshold.HasValue && (x.OnHandQuantity - x.ReservedQuantity) <= x.ReorderThreshold.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.ReorderThreshold.HasValue && (x.OnHandQuantity - x.ReservedQuantity) <= x.ReorderThreshold.Value)
            .ThenBy(x => x.DisplayName)
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(x => new InventoryItemSummaryDto(
                x.Id,
                x.StoreId,
                x.ProductId,
                x.ProductVariantId,
                x.Sku,
                x.DisplayName,
                x.OnHandQuantity,
                x.ReservedQuantity,
                x.OnHandQuantity - x.ReservedQuantity,
                x.ReorderThreshold,
                x.ReorderThreshold.HasValue && (x.OnHandQuantity - x.ReservedQuantity) <= x.ReorderThreshold.Value,
                x.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<InventoryItemSummaryDto>(
            items,
            criteria.PageNumber,
            criteria.PageSize,
            totalCount);
    }
}
