using Catalog.Application.Storefront.Queries.GetStorefrontProductBySlug;
using Catalog.Application.Storefront.Queries.SearchStorefrontProducts;
using ECommerce.API.Contracts.Catalog.Storefront;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Stores.Queries.GetStoreBySlug;

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
        var store = await _sender.Send(new GetStoreBySlugQuery(storeSlug), cancellationToken);
        if (store is null)
            return NotFound();

        var result = await _sender.Send(new SearchStorefrontProductsQuery(
            store.TenantId,
            request.CurrencyCode,
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
        [FromQuery] string currencyCode = "TRY",
        CancellationToken cancellationToken = default)
    {
        var store = await _sender.Send(new GetStoreBySlugQuery(storeSlug), cancellationToken);
        if (store is null)
            return NotFound();

        var product = await _sender.Send(new GetStorefrontProductBySlugQuery(store.TenantId, productSlug, currencyCode), cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }
}
