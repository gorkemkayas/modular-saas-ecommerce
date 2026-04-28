using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

public sealed class InventoryItemRepository : IInventoryItemRepository
{
    private readonly InventoryDbContext _context;

    public InventoryItemRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        await _context.InventoryItems.AddAsync(inventoryItem, cancellationToken);
    }

    public Task<InventoryItem?> GetByIdAsync(Guid storeId, Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        return _context.InventoryItems
            .Include(x => x.Reservations)
            .Include(x => x.Movements)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == inventoryItemId, cancellationToken);
    }

    public Task<InventoryItem?> GetBySellableItemAsync(Guid storeId, Guid productId, Guid? productVariantId, CancellationToken cancellationToken = default)
    {
        var sellableItemKey = InventoryItem.CreateSellableItemKey(productId, productVariantId);

        return _context.InventoryItems
            .Include(x => x.Reservations)
            .Include(x => x.Movements)
            .FirstOrDefaultAsync(
                x => x.StoreId == storeId && x.SellableItemKey == sellableItemKey,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryItem>> ListByReservationReferenceAsync(
        Guid storeId,
        string reservationReference,
        CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .Include(x => x.Reservations)
            .Include(x => x.Movements)
            .Where(x => x.StoreId == storeId && x.Reservations.Any(r => r.ReservationReference == reservationReference))
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> ExistsBySellableItemAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        Guid? excludedInventoryItemId = null,
        CancellationToken cancellationToken = default)
    {
        var sellableItemKey = InventoryItem.CreateSellableItemKey(productId, productVariantId);

        return _context.InventoryItems.AnyAsync(
            x => x.StoreId == storeId
                && x.SellableItemKey == sellableItemKey
                && (!excludedInventoryItemId.HasValue || x.Id != excludedInventoryItemId.Value),
            cancellationToken);
    }
}
