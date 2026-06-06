using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Store.ChangeStoreSlug;
using ECommerce.API.Contracts.Store.UpdateStoreProfile;
using ECommerce.API.Contracts.Store.UploadStoreHeroMedia;
using ECommerce.API.Integrations.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.DTOs;
using Store.Application.Stores.Commands.ChangeStoreSlug;
using Store.Application.Stores.Commands.PublishStore;
using Store.Application.Stores.Commands.UnpublishStore;
using Store.Application.Stores.Commands.UpdateStoreProfile;
using Store.Application.Stores.Queries.CheckStoreSlugAvailability;
using Store.Application.Stores.Queries.GetStoreByTenantId;
using Store.Application.Stores.Queries.SuggestAvailableSlug;
using Store.Domain.Stores;

namespace ECommerce.API.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = AppPolicies.TenantAdmin)]
    public class StoresController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly ITenantContext _tenantContext;
        private readonly IProductMediaStorageService _productMediaStorageService;

        public StoresController(
            ISender sender,
            ITenantContext tenantContext,
            IProductMediaStorageService productMediaStorageService)
        {
            _sender = sender;
            _tenantContext = tenantContext;
            _productMediaStorageService = productMediaStorageService;
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStore(CancellationToken cancellationToken)
        {
            var res = await _sender.Send(
                new GetStoreByTenantIdQuery(_tenantContext.TenantIdAsGuid!.Value),
                cancellationToken);

            return res is not null ? Ok(res) : NotFound();
        }

        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStoreProfile(
            [FromBody] UpdateStoreProfileRequest updateStoreProfileRequest,
            CancellationToken cancellationToken)
        {
            var command = new UpdateStoreProfileCommand(
                TenantId: _tenantContext.TenantIdAsGuid!.Value,
                Name: updateStoreProfileRequest.Name,
                Description: updateStoreProfileRequest.Description,
                LogoUrl: updateStoreProfileRequest.LogoUrl,
                HeroImageUrl: updateStoreProfileRequest.HeroImageUrl,
                HeroMediaType: ResolveHeroMediaType(updateStoreProfileRequest.HeroMediaType),
                HeroEyebrowText: updateStoreProfileRequest.HeroEyebrowText,
                HeroTitle: updateStoreProfileRequest.HeroTitle,
                HeroAccentTitle: updateStoreProfileRequest.HeroAccentTitle,
                HeroDescription: updateStoreProfileRequest.HeroDescription,
                HeroPrimaryButtonText: updateStoreProfileRequest.HeroPrimaryButtonText,
                LoginPageImageUrl: updateStoreProfileRequest.LoginPageImageUrl,
                RegisterPageImageUrl: updateStoreProfileRequest.RegisterPageImageUrl);

            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("hero-media/upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadStoreHeroMediaFileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> UploadStoreHeroMediaFile(
            [FromForm] UploadStoreHeroMediaFileRequest request,
            CancellationToken cancellationToken)
        {
            if (request.File is null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Hero media is required.",
                    Detail = "Select an image or video file before uploading."
                });
            }

            try
            {
                var storedMedia = await _productMediaStorageService.UploadAsync(
                    _tenantContext.TenantIdAsGuid!.Value,
                    request.File,
                    cancellationToken);

                return Ok(new UploadStoreHeroMediaFileResponse(
                    storedMedia.Url,
                    storedMedia.MediaType.ToString(),
                    storedMedia.OriginalFileName));
            }
            catch (InvalidOperationException exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
                {
                    Title = "Store hero media upload failed.",
                    Detail = exception.Message
                });
            }
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

        private static StorefrontHeroMediaType? ResolveHeroMediaType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return Enum.TryParse<StorefrontHeroMediaType>(value, true, out var mediaType)
                ? mediaType
                : null;
        }
    }
}
