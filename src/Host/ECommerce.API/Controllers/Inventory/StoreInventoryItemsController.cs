using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Inventory;
using Inventory.Application.Common.Models;
using Inventory.Application.InventoryItems.Commands.AddStock;
using Inventory.Application.InventoryItems.Commands.AdjustStock;
using Inventory.Application.InventoryItems.Commands.ConfirmInventoryDeduction;
using Inventory.Application.InventoryItems.Commands.CreateInventoryItem;
using Inventory.Application.InventoryItems.Commands.ReleaseInventoryReservation;
using Inventory.Application.InventoryItems.Commands.ReserveInventory;
using Inventory.Application.InventoryItems.Commands.SetReorderThreshold;
using Inventory.Application.InventoryItems.DTOs;
using Inventory.Application.InventoryItems.Queries.GetInventoryItemById;
using Inventory.Application.InventoryItems.Queries.GetInventoryMovements;
using Inventory.Application.InventoryItems.Queries.SearchInventoryItems;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Inventory;

[Route("api/stores/me/inventory/items")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class StoreInventoryItemsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StoreInventoryItemsController(ISender sender, ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InventoryItemSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchInventoryItems(
        [FromQuery] SearchInventoryItemsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SearchInventoryItemsQuery(
            GetStoreId(),
            request.ProductId,
            request.ProductVariantId,
            request.OnlyLowStock,
            request.SearchTerm,
            request.PageNumber,
            request.PageSize), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{inventoryItemId:guid}")]
    [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInventoryItemById([FromRoute] Guid inventoryItemId, CancellationToken cancellationToken)
    {
        var inventoryItem = await _sender.Send(new GetInventoryItemByIdQuery(GetStoreId(), inventoryItemId), cancellationToken);
        return inventoryItem is null ? NotFound() : Ok(inventoryItem);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateInventoryItem(
        [FromBody] CreateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var inventoryItemId = await _sender.Send(new CreateInventoryItemCommand(
            GetStoreId(),
            request.ProductId,
            request.ProductVariantId,
            request.InitialOnHandQuantity,
            request.ReorderThreshold), cancellationToken);

        return CreatedAtAction(nameof(GetInventoryItemById), new { inventoryItemId }, null);
    }

    [HttpPost("{inventoryItemId:guid}/stock/add")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddStock(
        [FromRoute] Guid inventoryItemId,
        [FromBody] AddStockRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new AddStockCommand(
            GetStoreId(),
            inventoryItemId,
            request.Quantity,
            request.Reason,
            request.Reference), cancellationToken);

        return NoContent();
    }

    [HttpPut("{inventoryItemId:guid}/stock/adjust")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AdjustStock(
        [FromRoute] Guid inventoryItemId,
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new AdjustStockCommand(
            GetStoreId(),
            inventoryItemId,
            request.NewOnHandQuantity,
            request.Reason,
            request.Reference), cancellationToken);

        return NoContent();
    }

    [HttpPut("{inventoryItemId:guid}/reorder-threshold")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetReorderThreshold(
        [FromRoute] Guid inventoryItemId,
        [FromBody] SetReorderThresholdRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new SetReorderThresholdCommand(GetStoreId(), inventoryItemId, request.ReorderThreshold), cancellationToken);
        return NoContent();
    }

    [HttpPost("reservations")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReserveInventory(
        [FromBody] ReserveInventoryRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ReserveInventoryCommand(
            GetStoreId(),
            request.OrderId,
            request.ReservationReference,
            request.Items
                .Select(x => new ReserveInventoryItemInput(x.ProductId, x.ProductVariantId, x.Quantity))
                .ToArray()), cancellationToken);

        return NoContent();
    }

    [HttpPost("reservations/{reservationReference}/release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReleaseReservation(
        [FromRoute] string reservationReference,
        [FromBody] ReleaseInventoryReservationRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ReleaseInventoryReservationCommand(
            GetStoreId(),
            reservationReference,
            request.Reason), cancellationToken);

        return NoContent();
    }

    [HttpPost("reservations/{reservationReference}/confirm-deduction")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmDeduction(
        [FromRoute] string reservationReference,
        [FromBody] ConfirmInventoryDeductionRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ConfirmInventoryDeductionCommand(
            GetStoreId(),
            reservationReference,
            request.Reason), cancellationToken);

        return NoContent();
    }

    [HttpGet("{inventoryItemId:guid}/movements")]
    [ProducesResponseType(typeof(PagedResult<StockMovementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovements(
        [FromRoute] Guid inventoryItemId,
        [FromQuery] GetInventoryMovementsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInventoryMovementsQuery(
            GetStoreId(),
            inventoryItemId,
            request.PageNumber,
            request.PageSize), cancellationToken);

        return Ok(result);
    }

    private Guid GetStoreId()
    {
        return _tenantContext.TenantIdAsGuid
            ?? throw new InvalidOperationException("Tenant context is not available.");
    }
}
