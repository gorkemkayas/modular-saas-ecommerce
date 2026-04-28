using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories;

public interface IInventoryItemRepository
{
    Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByIdAsync(Guid storeId, Guid inventoryItemId, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetBySellableItemAsync(Guid storeId, Guid productId, Guid? productVariantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryItem>> ListByReservationReferenceAsync(Guid storeId, string reservationReference, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySellableItemAsync(Guid storeId, Guid productId, Guid? productVariantId, Guid? excludedInventoryItemId = null, CancellationToken cancellationToken = default);
}
