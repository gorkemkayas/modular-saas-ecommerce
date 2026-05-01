using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Shipment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipment.Application.Shipments.Commands.CreateShipment;
using Shipment.Application.Shipments.DTOs;
using Shipment.Application.Shipments.Queries.GetStoreOrderShipments;

namespace ECommerce.API.Controllers.Shipment;

[Route("api/stores/me/orders/{orderId:guid}/shipments")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class StoreOrderShipmentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StoreOrderShipmentsController(
        ISender sender,
        ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ShipmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByOrder(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var shipments = await _sender.Send(
            new GetStoreOrderShipmentsQuery(GetStoreId(), orderId),
            cancellationToken);

        return Ok(shipments);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid orderId,
        [FromBody] CreateShipmentRequest request,
        CancellationToken cancellationToken)
    {
        var shipmentId = await _sender.Send(
            new CreateShipmentCommand(GetStoreId(), orderId, request.InternalNote),
            cancellationToken);

        return CreatedAtRoute("GetStoreShipmentById", new { shipmentId }, new { ShipmentId = shipmentId });
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid ?? Guid.Empty;
}
