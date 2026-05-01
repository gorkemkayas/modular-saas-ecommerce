using BuildingBlocks.Application.Abstractions.Authentication;
using BuildingBlocks.Application.Abstractions.Tenancy;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipment.Application.Shipments.DTOs;
using Shipment.Application.Shipments.Queries.GetMyOrderShipmentById;
using Shipment.Application.Shipments.Queries.GetMyOrderShipments;

namespace ECommerce.API.Controllers.Shipment;

[Route("api/orders/me/{orderId:guid}/shipments")]
[ApiController]
[Authorize]
public sealed class OrderShipmentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public OrderShipmentsController(
        ISender sender,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _sender = sender;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ShipmentSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListByOrder(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var storeId, out var externalUserId))
            return Unauthorized();

        var shipments = await _sender.Send(
            new GetMyOrderShipmentsQuery(storeId, externalUserId, orderId),
            cancellationToken);

        return Ok(shipments);
    }

    [HttpGet("{shipmentId:guid}")]
    [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid orderId,
        [FromRoute] Guid shipmentId,
        CancellationToken cancellationToken)
    {
        if (!TryGetContext(out var storeId, out var externalUserId))
            return Unauthorized();

        var shipment = await _sender.Send(
            new GetMyOrderShipmentByIdQuery(storeId, externalUserId, orderId, shipmentId),
            cancellationToken);

        return shipment is null ? NotFound() : Ok(shipment);
    }

    private bool TryGetContext(out Guid storeId, out Guid externalUserId)
    {
        storeId = _tenantContext.TenantIdAsGuid ?? Guid.Empty;
        externalUserId = _currentUser.UserId ?? Guid.Empty;

        return storeId != Guid.Empty && externalUserId != Guid.Empty && _currentUser.IsAuthenticated;
    }
}
