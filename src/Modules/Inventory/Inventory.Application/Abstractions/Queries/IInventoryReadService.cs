using Inventory.Application.Common.Models;
using Inventory.Application.InventoryItems.DTOs;

namespace Inventory.Application.Abstractions.Queries;

public interface IInventoryReadService
{
    Task<InventoryItemDto?> GetByIdAsync(Guid storeId, Guid inventoryItemId, CancellationToken cancellationToken = default);

    Task<PagedResult<InventoryItemSummaryDto>> SearchAsync(
        InventoryItemSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<PagedResult<StockMovementDto>> GetMovementsAsync(
        Guid storeId,
        Guid inventoryItemId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
