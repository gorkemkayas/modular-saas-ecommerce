using Catalog.Application.Storefront.Queries.GetStorefrontProductBySlug;
using Catalog.Application.Storefront.Queries.SearchStorefrontProducts;
using ECommerce.API.Contracts.Catalog.Storefront;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Stores.Queries.GetPublishedStorefrontBySlug;

namespace ECommerce.API.Controllers.Catalog.Storefront;

[Route("api/storefront/{storeSlug}/products")]
[ApiController]
public sealed class StorefrontProductsController : ControllerBase
{
    private readonly ISender _sender;

    public StorefrontProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchProducts(
        [FromRoute] string storeSlug,
        [FromQuery] StorefrontProductSearchRequest request,
        CancellationToken cancellationToken)
    {
        var storefront = await _sender.Send(new GetPublishedStoreFrontBySlugQuery(storeSlug), cancellationToken);
        if (storefront is null)
            return NotFound();

        var result = await _sender.Send(new SearchStorefrontProductsQuery(
            storefront.TenantId,
            request.SearchTerm,
            request.CategoryId,
            request.BrandId,
            request.PageNumber,
            request.PageSize), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{productSlug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductBySlug(
        [FromRoute] string storeSlug,
        [FromRoute] string productSlug,
        CancellationToken cancellationToken)
    {
        var storefront = await _sender.Send(new GetPublishedStoreFrontBySlugQuery(storeSlug), cancellationToken);
        if (storefront is null)
            return NotFound();

        var product = await _sender.Send(new GetStorefrontProductBySlugQuery(storefront.TenantId, productSlug), cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }
}
