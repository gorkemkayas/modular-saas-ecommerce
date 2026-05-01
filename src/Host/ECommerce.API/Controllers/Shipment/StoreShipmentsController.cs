using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Shipment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipment.Application.Common.Models;
using Shipment.Application.Shipments.Commands.AddShipmentPackage;
using Shipment.Application.Shipments.Commands.AssignShipmentCarrier;
using Shipment.Application.Shipments.Commands.CancelShipment;
using Shipment.Application.Shipments.Commands.MarkShipmentDelivered;
using Shipment.Application.Shipments.Commands.MarkShipmentDeliveryException;
using Shipment.Application.Shipments.Commands.MarkShipmentReadyForDispatch;
using Shipment.Application.Shipments.Commands.MarkShipmentShipped;
using Shipment.Application.Shipments.Commands.RegisterShipmentTrackingEvent;
using Shipment.Application.Shipments.DTOs;
using Shipment.Application.Shipments.Queries.GetStoreShipmentById;
using Shipment.Application.Shipments.Queries.SearchStoreShipments;

namespace ECommerce.API.Controllers.Shipment;

[Route("api/stores/me/shipments")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
[ApiExplorerSettings(GroupName = "v1")]
public sealed class StoreShipmentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StoreShipmentsController(
        ISender sender,
        ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ShipmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] SearchShipmentsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SearchStoreShipmentsQuery(
                GetStoreId(),
                request.Status,
                request.OrderId,
                request.OrderNumber,
                request.ShipmentNumber,
                request.TrackingNumber,
                request.PageNumber,
                request.PageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{shipmentId:guid}", Name = "GetStoreShipmentById")]
    [ProducesResponseType(typeof(ShipmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid shipmentId,
        CancellationToken cancellationToken)
    {
        var shipment = await _sender.Send(
            new GetStoreShipmentByIdQuery(GetStoreId(), shipmentId),
            cancellationToken);

        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpPost("{shipmentId:guid}/packages")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddPackage(
        [FromRoute] Guid shipmentId,
        [FromBody] AddShipmentPackageRequest request,
        CancellationToken cancellationToken)
    {
        var packageId = await _sender.Send(
            new AddShipmentPackageCommand(
                GetStoreId(),
                shipmentId,
                request.TrackingNumber,
                request.Weight,
                request.WeightUnit,
                request.LabelReference),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { shipmentId }, new { PackageId = packageId });
    }

    [HttpPut("{shipmentId:guid}/carrier")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignCarrier(
        [FromRoute] Guid shipmentId,
        [FromBody] AssignShipmentCarrierRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new AssignShipmentCarrierCommand(
                GetStoreId(),
                shipmentId,
                request.CarrierCode,
                request.CarrierName,
                request.ServiceCode,
                request.ServiceName,
                request.TrackingUrl),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/ready")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkReady(
        [FromRoute] Guid shipmentId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new MarkShipmentReadyForDispatchCommand(GetStoreId(), shipmentId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkShipped(
        [FromRoute] Guid shipmentId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new MarkShipmentShippedCommand(GetStoreId(), shipmentId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/tracking-events")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RegisterTrackingEvent(
        [FromRoute] Guid shipmentId,
        [FromBody] RegisterShipmentTrackingEventRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new RegisterShipmentTrackingEventCommand(
                GetStoreId(),
                shipmentId,
                request.PackageId,
                request.Type,
                request.OccurredAtUtc,
                request.Location,
                request.Description,
                request.RawStatusCode,
                request.RawStatusText),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/deliver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkDelivered(
        [FromRoute] Guid shipmentId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new MarkShipmentDeliveredCommand(GetStoreId(), shipmentId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/delivery-exception")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkDeliveryException(
        [FromRoute] Guid shipmentId,
        [FromBody] MarkShipmentDeliveryExceptionRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new MarkShipmentDeliveryExceptionCommand(
                GetStoreId(),
                shipmentId,
                request.PackageId,
                request.Description,
                request.Location,
                request.RawStatusCode,
                request.RawStatusText),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid shipmentId,
        [FromBody] CancelShipmentRequest? request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new CancelShipmentCommand(GetStoreId(), shipmentId, request?.Reason), cancellationToken);
        return NoContent();
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid ?? Guid.Empty;
}
