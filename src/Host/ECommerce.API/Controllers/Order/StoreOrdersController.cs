using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Orders.DTOs;
using Order.Application.Orders.Queries.GetStoreOrderById;

namespace ECommerce.API.Controllers.Order;

[Route("api/stores/me/orders")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class StoreOrdersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StoreOrdersController(
        ISender sender,
        ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var storeId = _tenantContext.TenantIdAsGuid ?? Guid.Empty;
        var order = await _sender.Send(new GetStoreOrderByIdQuery(storeId, orderId), cancellationToken);

        return order is null ? NotFound() : Ok(order);
    }
}
