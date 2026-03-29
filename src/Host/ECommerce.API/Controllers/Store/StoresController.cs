using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Store.ChangeStoreSlug;
using ECommerce.API.Contracts.Store.UpdateStoreProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Stores.Commands.ChangeStoreSlug;
using Store.Application.Stores.Commands.PublishStore;
using Store.Application.Stores.Commands.UnpublishStore;
using Store.Application.Stores.Commands.UpdateStoreProfile;
using Store.Application.Stores.Queries.CheckStoreSlugAvailability;
using Store.Application.Stores.Queries.GetStoreByTenantId;
using Store.Application.Stores.Queries.SuggestAvailableSlug;
using Store.Application.DTOs;

namespace ECommerce.API.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = AppPolicies.TenantAdmin)]
    public class StoresController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly ITenantContext _tenantContext;
        public StoresController(ISender sender, ITenantContext tenantContext)
        {
            _sender = sender;
            _tenantContext = tenantContext;
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStore(CancellationToken cancellationToken)
        {
            var res = await _sender.Send(new GetStoreByTenantIdQuery(_tenantContext.TenantIdAsGuid!.Value), cancellationToken);

            return res is not null ? Ok(res) : NotFound();
        }

        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStoreProfile([FromBody] UpdateStoreProfileRequest updateStoreProfileRequest, CancellationToken cancellationToken)
        {
            var command = new UpdateStoreProfileCommand(
                TenantId: _tenantContext.TenantIdAsGuid!.Value,
                Name: updateStoreProfileRequest.Name,
                Description: updateStoreProfileRequest.Description,
                LogoUrl: updateStoreProfileRequest.LogoUrl
            );
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPut("slug")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ChangeStoreSlug([FromQuery] string newSlug, CancellationToken cancellationToken)
        {
            await _sender.Send(new ChangeStoreSlugCommand(_tenantContext.TenantIdAsGuid!.Value, newSlug), cancellationToken);
            return NoContent();
        }

        [HttpPost("publish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PublishStore(CancellationToken cancellationToken)
        {
            await _sender.Send(new PublishStoreCommand(_tenantContext.TenantIdAsGuid!.Value), cancellationToken);
            return NoContent();
        }

        [HttpPost("unpublish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnpublishStore(CancellationToken cancellationToken)
        {
            await _sender.Send(new UnpublishStoreCommand(_tenantContext.TenantIdAsGuid!.Value), cancellationToken);
            return NoContent();
        }

        [HttpGet("slug-availability")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckStoreSlugAvailability([FromQuery] string slug, CancellationToken cancellationToken)
        {
            var isAvailable = await _sender.Send(new CheckStoreSlugAvailabilityQuery(slug), cancellationToken);
            return Ok(new { Slug = slug, IsAvailable = isAvailable });
        }

        [HttpGet("suggest-slug")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> SuggestAvailableSlug([FromQuery] string slug, CancellationToken cancellationToken)
        {
            var suggestedSlug = await _sender.Send(new SuggestAvailableSlugQuery(slug), cancellationToken);
            return Ok(new { Slug = suggestedSlug });
        }

    }
}
