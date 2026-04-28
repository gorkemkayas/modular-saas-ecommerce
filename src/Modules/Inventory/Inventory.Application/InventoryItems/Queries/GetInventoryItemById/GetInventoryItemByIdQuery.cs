using Inventory.Application.InventoryItems.DTOs;
using MediatR;

namespace Inventory.Application.InventoryItems.Queries.GetInventoryItemById;

public sealed record GetInventoryItemByIdQuery(Guid StoreId, Guid InventoryItemId) : IRequest<InventoryItemDto?>;
