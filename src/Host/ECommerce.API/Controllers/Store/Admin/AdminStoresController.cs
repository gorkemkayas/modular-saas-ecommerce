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
using Store.Application.DTOs;
using Store.Application.Stores.Queries.SuggestAvailableSlug;
using Subscription.Application.Commands.ProvisionTenantSubscription;
using Subscription.Application.Queries.GetPublicPlans;
using Subscription.Contracts;

namespace ECommerce.API.Controllers.Store.Admin;

[Route("api/admin/stores")]
[ApiController]
[Authorize(Policy =AppPolicies.SuperAdmin)]
public sealed class AdminStoresController : ControllerBase
{
    private readonly ISender _sender;
    private readonly string _serviceToken; // mvp için geçici çözüm, OAuth2 Credentials Flow ile güncellenecek.
    public AdminStoresController(ISender sender, IConfiguration configuration)
    {
        _sender = sender;
        _serviceToken = configuration["ServiceTokens:ECommerce"]!;
    }

    [HttpGet("by-tenant/{tenantId:int}")]
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStoreByTenantId([FromRoute] int tenantId, CancellationToken cancellationToken)
    {
        var store = await _sender.Send(new GetStoreByTenantIdQuery(TenantIdConverter.ToGuid(tenantId)));

        if (store is null)
            return NotFound();
        return Ok(store);
    }

    [HttpGet("by-id/{storeId:guid}")]
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStoreById([FromRoute] Guid storeId, CancellationToken cancellationToken)
    {
        var store = await _sender.Send(new GetStoreByIdQuery(storeId), cancellationToken);
        if (store is null)
            return NotFound();
        return Ok(store);
    }
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStoreBySlug([FromRoute] string slug, CancellationToken cancellationToken)
    {
        var store = await _sender.Send(new GetStoreBySlugQuery(slug), cancellationToken);
        if (store is null)
            return NotFound();
        return Ok(store);
    }

    [HttpPost("{tenantId:int}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateStore([FromRoute] int tenantId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateStoreCommand(TenantIdConverter.ToGuid(tenantId)), cancellationToken);
        return NoContent();
    }

    [HttpPost("{tenantId:int}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendStore([FromRoute] int tenantId, CancellationToken cancellationToken)
    {
        await _sender.Send(new SuspendStoreCommand(TenantIdConverter.ToGuid(tenantId)), cancellationToken);
        return NoContent();
    }

    [HttpPost("{tenantId:int}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveStore([FromRoute] int tenantId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ArchiveStoreCommand(TenantIdConverter.ToGuid(tenantId)), cancellationToken);
        return NoContent();
    }

    [HttpPost("{tenantId:int}/provision")]
    [ProducesResponseType(typeof(ProvisionStoreForTenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [AllowAnonymous] // mvp için hızlı bir yol, mvp den sonra OAuth2 Credentials Flow ile güncellenecek.
    public async Task<IActionResult> ProvisionStoreForTenant([FromRoute] int tenantId, [FromBody] ProvisionStoreForTenantRequest request, CancellationToken cancellationToken, [FromHeader(Name = "Authorization")] string authHeader)
    {
        var token = authHeader?.Replace("Bearer ", "");
        if (token != _serviceToken)
            return Unauthorized();

        var tenantGuid = TenantIdConverter.ToGuid(tenantId);
        var planCode = string.IsNullOrWhiteSpace(request.PlanCode)
            ? SubscriptionPlanCodes.Starter
            : request.PlanCode.Trim().ToLowerInvariant();

        var publicPlans = await _sender.Send(new GetPublicPlansQuery(), cancellationToken);
        if (!publicPlans.Any(plan => string.Equals(plan.Code, planCode, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid subscription plan.",
                Detail = $"Subscription plan '{planCode}' is not available."
            });
        }

        var suggestion = await _sender.Send(new SuggestAvailableSlugQuery(request.Name), cancellationToken);
        var storeId = await _sender.Send(new ProvisionStoreForTenantCommand(tenantGuid, request.Name, suggestion.Slug), cancellationToken);

        await _sender.Send(new ProvisionTenantSubscriptionCommand(tenantGuid, planCode), cancellationToken);

        return CreatedAtAction(
            nameof(GetStoreById),
            new { storeId },
            new ProvisionStoreForTenantResponse(storeId.ToString(), suggestion.Slug));
    }
}
