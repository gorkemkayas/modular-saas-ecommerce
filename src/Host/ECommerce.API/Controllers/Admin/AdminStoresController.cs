using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Store.ProvisionStoreForTenant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Stores.Commands.ActivateStore;
using Store.Application.Stores.Commands.ArchiveStore;
using Store.Application.Stores.Commands.ProvisionStoreForTenant;
using Store.Application.Stores.Commands.SuspendStore;
using Store.Application.Stores.Queries.GetStoreById;
using Store.Application.Stores.Queries.GetStoreBySlug;
using Store.Application.Stores.Queries.GetStoreByTenantId;

namespace ECommerce.API.Controllers.Admin;

[Route("api/admin/stores")]
[ApiController]
[Authorize(Policy =AppPolicies.SuperAdmin)]
public sealed class AdminStoresController : ControllerBase
{
    private readonly ISender _sender;
    public AdminStoresController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("by-tenant/{tenantId:int}")]
    public async Task<IActionResult> GetStoreByTenantId([FromRoute] int tenantId, CancellationToken cancellationToken)
    {
        var store = await _sender.Send(new GetStoreByTenantIdQuery(TenantIdConverter.ToGuid(tenantId)));

        if (store is null)
            return NotFound();
        return Ok(store);
    }

    [HttpGet("by-id/{storeId:guid}")]
    public async Task<IActionResult> GetStoreById([FromRoute] Guid storeId, CancellationToken cancellationToken)
    {
        var store = await _sender.Send(new GetStoreByIdQuery(storeId), cancellationToken);
        if (store is null)
            return NotFound();
        return Ok(store);
    }
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetStoreBySlug([FromRoute] string slug, CancellationToken cancellationToken)
    {
        var store = await _sender.Send(new GetStoreBySlugQuery(slug), cancellationToken);
        if (store is null)
            return NotFound();
        return Ok(store);
    }

    [HttpPost("{tenantId:int}/activate")]
    public async Task<IActionResult> ActivateStore([FromRoute] int tenantId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateStoreCommand(TenantIdConverter.ToGuid(tenantId)), cancellationToken);
        return NoContent();
    }

    [HttpPost("{tenantId:int}/suspend")]
    public async Task<IActionResult> SuspendStore([FromRoute] int tenantId, CancellationToken cancellationToken)
    {
        await _sender.Send(new SuspendStoreCommand(TenantIdConverter.ToGuid(tenantId)), cancellationToken);
        return NoContent();
    }

    [HttpPost("{tenantId:int}/archive")]
    public async Task<IActionResult> ArchiveStore([FromRoute] int tenantId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ArchiveStoreCommand(TenantIdConverter.ToGuid(tenantId)), cancellationToken);
        return NoContent();
    }

    [HttpPost("{tenantId:int}/provision")]
    public async Task<IActionResult> ProvisionStoreForTenant([FromRoute] int tenantId, [FromBody] ProvisionStoreForTenantRequest request, CancellationToken cancellationToken)
    {
        var storeId = await _sender.Send(new ProvisionStoreForTenantCommand(TenantIdConverter.ToGuid(tenantId), request.Name, request.Slug), cancellationToken);
        return CreatedAtAction(nameof(GetStoreById), new { storeId = storeId }, null);
    }
}
