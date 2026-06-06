using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shipment.Application.ShippingCarriers.DTOs;
using Shipment.Application.ShippingCarriers.Queries.ListShippingCarriers;
using Store.Application.DTOs;
using Store.Application.Stores.Queries.GetStoreBySlug;

namespace ECommerce.API.Controllers.Store;

[Route("api/[controller]")]
[ApiController]
public sealed class StorefrontController : ControllerBase
{
    private readonly ISender _sender;

    public StorefrontController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("status/{slug}")]
    [ProducesResponseType(typeof(StoreTenantStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStoreTenantStatusBySlug([FromRoute] string slug, CancellationToken cancellationToken)
    {
        var store = await _sender.Send(new GetStoreBySlugQuery(slug), cancellationToken);
        if (store is null)
            return NotFound();

        return Ok(new StoreTenantStatusResponse(
            store.Id,
            store.TenantId,
            store.Name,
            store.Slug,
            store.Description,
            store.LogoUrl,
            store.Status.ToString(),
            store.IsPublished));
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(StorefrontDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublishedStoreFrontBySlug([FromRoute] string slug, CancellationToken cancellationToken)
    {
        var store = await _sender.Send(new GetStoreBySlugQuery(slug), cancellationToken);
        if (store is null)
            return NotFound();

        return Ok(new StorefrontDto(
            store.TenantId,
            store.Name,
            store.Slug,
            store.Description,
            store.LogoUrl,
            store.HeroImageUrl,
            store.HeroMediaType,
            store.HeroEyebrowText,
            store.HeroTitle,
            store.HeroAccentTitle,
            store.HeroDescription,
            store.HeroPrimaryButtonText,
            store.LoginPageImageUrl,
            store.RegisterPageImageUrl));
    }

    [HttpGet("{slug}/shipping-carriers")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ShippingCarrierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublishedShippingCarriersBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var store = await _sender.Send(new GetStoreBySlugQuery(slug), cancellationToken);
        if (store is null)
            return NotFound();

        var carriers = await _sender.Send(
            new ListShippingCarriersQuery(store.TenantId, ActiveOnly: true),
            cancellationToken);

        return Ok(carriers);
    }

    public sealed record StoreTenantStatusResponse(
        Guid Id,
        Guid TenantId,
        string Name,
        string Slug,
        string? Description,
        string? LogoUrl,
        string Status,
        bool IsPublished);
}
