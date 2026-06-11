using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subscription.Application.DTOs;
using Subscription.Application.Queries.GetTenantSubscription;

namespace ECommerce.API.Controllers.Subscription;

[Route("api/subscription")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class CurrentSubscriptionController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public CurrentSubscriptionController(ISender sender, ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(TenantSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentSubscription(CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantIdAsGuid ?? Guid.Empty;
        var subscription = await _sender.Send(
            new GetTenantSubscriptionQuery(tenantId),
            cancellationToken);

        if (subscription is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Subscription not found",
                Detail = "No subscription has been provisioned for the current tenant."
            });
        }

        return Ok(subscription);
    }
}
