using Catalog.Application.Storefront.Queries.GetStorefrontCatalogFacets;
using Catalog.Application.Storefront.Queries.GetStorefrontCategoryTree;
using Catalog.Application.Storefront.Queries.SearchStorefrontBrands;
using ECommerce.API.Contracts.Catalog.Storefront;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Stores.Queries.GetPublishedStorefrontBySlug;

namespace ECommerce.API.Controllers.Catalog.Storefront;

[Route("api/storefront/{storeSlug}")]
[ApiController]
public sealed class StorefrontCatalogController : ControllerBase
{
    private readonly ISender _sender;

    public StorefrontCatalogController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("categories/tree")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryTree([FromRoute] string storeSlug, CancellationToken cancellationToken)
    {
        var storefront = await _sender.Send(new GetPublishedStoreFrontBySlugQuery(storeSlug), cancellationToken);
        if (storefront is null)
            return NotFound();

        var categories = await _sender.Send(new GetStorefrontCategoryTreeQuery(storefront.TenantId), cancellationToken);
        return Ok(categories);
    }

    [HttpGet("brands")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchBrands(
        [FromRoute] string storeSlug,
        [FromQuery] StorefrontBrandSearchRequest request,
        CancellationToken cancellationToken)
    {
        var storefront = await _sender.Send(new GetPublishedStoreFrontBySlugQuery(storeSlug), cancellationToken);
        if (storefront is null)
            return NotFound();

        var brands = await _sender.Send(new SearchStorefrontBrandsQuery(storefront.TenantId, request.SearchTerm), cancellationToken);
        return Ok(brands);
    }

    [HttpGet("facets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCatalogFacets(
        [FromRoute] string storeSlug,
        [FromQuery] StorefrontCatalogFacetsRequest request,
        CancellationToken cancellationToken)
    {
        var storefront = await _sender.Send(new GetPublishedStoreFrontBySlugQuery(storeSlug), cancellationToken);
        if (storefront is null)
            return NotFound();

        var facets = await _sender.Send(new GetStorefrontCatalogFacetsQuery(
            storefront.TenantId,
            request.SearchTerm,
            request.CategoryId,
            request.BrandId), cancellationToken);

        return Ok(facets);
    }
}
