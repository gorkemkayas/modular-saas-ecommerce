using Inventory.Application.Common.Models;
using Inventory.Application.InventoryItems.DTOs;
using MediatR;

namespace Inventory.Application.InventoryItems.Queries.GetInventoryMovements;

public sealed record GetInventoryMovementsQuery(
    Guid StoreId,
    Guid InventoryItemId,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<StockMovementDto>>;
