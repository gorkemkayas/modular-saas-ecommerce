using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subscription.Application.DTOs;
using Subscription.Application.Queries.GetPublicPlans;

namespace ECommerce.API.Controllers.Subscription;

[Route("api/plans")]
[ApiController]
[AllowAnonymous]
public sealed class PlansController : ControllerBase
{
    private readonly ISender _sender;

    public PlansController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<PlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPublicPlans(CancellationToken cancellationToken)
    {
        var plans = await _sender.Send(new GetPublicPlansQuery(), cancellationToken);

        return Ok(plans);
    }
}
