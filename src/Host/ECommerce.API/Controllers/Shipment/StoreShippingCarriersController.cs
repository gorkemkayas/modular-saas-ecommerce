using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Shipment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipment.Application.ShippingCarriers.Commands.CreateShippingCarrier;
using Shipment.Application.ShippingCarriers.Commands.UpdateShippingCarrier;
using Shipment.Application.ShippingCarriers.DTOs;
using Shipment.Application.ShippingCarriers.Queries.ListShippingCarriers;

namespace ECommerce.API.Controllers.Shipment;

[Route("api/stores/me/shipping-carriers")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
[ApiExplorerSettings(GroupName = "v1")]
public sealed class StoreShippingCarriersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StoreShippingCarriersController(
        ISender sender,
        ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ShippingCarrierDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] bool activeOnly,
        CancellationToken cancellationToken)
    {
        var carriers = await _sender.Send(
            new ListShippingCarriersQuery(GetStoreId(), activeOnly),
            cancellationToken);

        return Ok(carriers);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateShippingCarrierRequest request,
        CancellationToken cancellationToken)
    {
        var carrierId = await _sender.Send(
            new CreateShippingCarrierCommand(
                GetStoreId(),
                request.Code,
                request.Name,
                request.ServiceCode,
                request.ServiceName,
                request.TrackingUrl,
                request.SortOrder),
            cancellationToken);

        return CreatedAtAction(nameof(List), new { activeOnly = false }, new { CarrierId = carrierId });
    }

    [HttpPut("{carrierId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid carrierId,
        [FromBody] UpdateShippingCarrierRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateShippingCarrierCommand(
                GetStoreId(),
                carrierId,
                request.Code,
                request.Name,
                request.ServiceCode,
                request.ServiceName,
                request.TrackingUrl,
                request.IsActive,
                request.SortOrder),
            cancellationToken);

        return NoContent();
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid ?? Guid.Empty;
}
