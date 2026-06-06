using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using Catalog.Application.Common.Models;
using Catalog.Application.Products.Commands.ActivateProduct;
using Catalog.Application.Products.Commands.AddProductMedia;
using Catalog.Application.Products.Commands.AddVariant;
using Catalog.Application.Products.Commands.ArchiveProduct;
using Catalog.Application.Products.Commands.AssignProductCategories;
using Catalog.Application.Products.Commands.ChangeProductSlug;
using Catalog.Application.Products.Commands.CreateSimpleProduct;
using Catalog.Application.Products.Commands.CreateVariantProduct;
using Catalog.Application.Products.Commands.PublishProduct;
using Catalog.Application.Products.Commands.SetProductAttributes;
using Catalog.Application.Products.Commands.UnpublishProduct;
using Catalog.Application.Products.Commands.UpdateProductDetails;
using Catalog.Application.Products.DTOs;
using Catalog.Application.Products.Queries.GetProductById;
using Catalog.Application.Products.Queries.GetProductBySlug;
using Catalog.Application.Products.Queries.SearchProducts;
using ECommerce.API.Contracts.Catalog.Products;
using ECommerce.API.Integrations.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Catalog.Stores;

[Route("api/stores/me/products")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class StoreProductsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;
    private readonly IProductMediaStorageService _productMediaStorageService;

    public StoreProductsController(
        ISender sender,
        ITenantContext tenantContext,
        IProductMediaStorageService productMediaStorageService)
    {
        _sender = sender;
        _tenantContext = tenantContext;
        _productMediaStorageService = productMediaStorageService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts([FromQuery] StoreProductSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SearchProductsQuery(
            GetStoreId(),
            request.SearchTerm,
            request.Status,
            request.ProductType,
            request.IsPublished,
            request.CategoryId,
            request.BrandId,
            request.PageNumber,
            request.PageSize), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById([FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductByIdQuery(GetStoreId(), productId), cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductBySlug([FromRoute] string slug, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductBySlugQuery(GetStoreId(), slug), cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost("simple")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSimpleProduct([FromBody] CreateSimpleProductRequest request, CancellationToken cancellationToken)
    {
        var productId = await _sender.Send(new CreateSimpleProductCommand(
            GetStoreId(),
            request.Name,
            request.Slug,
            request.Sku,
            request.ShortDescription,
            request.Description,
            request.BrandId,
            request.CategoryIds), cancellationToken);

        return CreatedAtAction(nameof(GetProductById), new { productId }, null);
    }

    [HttpPost("variant")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateVariantProduct([FromBody] CreateVariantProductRequest request, CancellationToken cancellationToken)
    {
        var productId = await _sender.Send(new CreateVariantProductCommand(
            GetStoreId(),
            request.Name,
            request.Slug,
            request.ShortDescription,
            request.Description,
            request.BrandId,
            request.CategoryIds), cancellationToken);

        return CreatedAtAction(nameof(GetProductById), new { productId }, null);
    }

    [HttpPut("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateProductDetails(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductDetailsRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateProductDetailsCommand(
            GetStoreId(),
            productId,
            request.Name,
            request.ShortDescription,
            request.Description,
            request.BrandId), cancellationToken);

        return NoContent();
    }

    [HttpPut("{productId:guid}/slug")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeProductSlug(
        [FromRoute] Guid productId,
        [FromBody] ChangeProductSlugRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ChangeProductSlugCommand(GetStoreId(), productId, request.Slug), cancellationToken);
        return NoContent();
    }

    [HttpPut("{productId:guid}/categories")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignProductCategories(
        [FromRoute] Guid productId,
        [FromBody] AssignProductCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new AssignProductCategoriesCommand(GetStoreId(), productId, request.CategoryIds), cancellationToken);
        return NoContent();
    }

    [HttpPut("{productId:guid}/attributes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetProductAttributes(
        [FromRoute] Guid productId,
        [FromBody] SetProductAttributesRequest request,
        CancellationToken cancellationToken)
    {
        var attributeValues = request.AttributeValues
            .Select(x => new ProductAttributeValueInput(x.AttributeDefinitionId, x.Value))
            .ToArray();

        await _sender.Send(new SetProductAttributesCommand(GetStoreId(), productId, attributeValues), cancellationToken);
        return NoContent();
    }

    [HttpPost("{productId:guid}/variants")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddVariant(
        [FromRoute] Guid productId,
        [FromBody] AddVariantRequest request,
        CancellationToken cancellationToken)
    {
        var variantId = await _sender.Send(new AddVariantCommand(
            GetStoreId(),
            productId,
            request.Sku,
            request.Name,
            request.SortOrder,
            request.AttributeValues
                .Select(x => new VariantAttributeValueInput(x.AttributeDefinitionId, x.Value))
                .ToArray()), cancellationToken);

        return CreatedAtAction(nameof(GetProductById), new { productId }, new { VariantId = variantId });
    }

    [HttpPost("{productId:guid}/media")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddProductMedia(
        [FromRoute] Guid productId,
        [FromBody] AddProductMediaRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new AddProductMediaCommand(
            GetStoreId(),
            productId,
            request.MediaType,
            request.Url,
            request.AltText,
            request.IsMain,
            request.SortOrder,
            request.ProductVariantId), cancellationToken);

        return NoContent();
    }

    [HttpPost("media/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadProductMediaFileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadProductMediaFile(
        [FromForm] UploadProductMediaFileRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length <= 0)
            return BadRequest(new ProblemDetails { Detail = "A product media file is required." });

        try
        {
            var storedMedia = await _productMediaStorageService.UploadAsync(
                GetStoreId(),
                request.File,
                cancellationToken);

            return Ok(new UploadProductMediaFileResponse(
                storedMedia.Url,
                storedMedia.MediaType,
                storedMedia.OriginalFileName));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Title = "Product media upload is unavailable.",
                    Detail = ex.Message
                });
        }
    }

    [HttpPost("{productId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivateProduct([FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateProductCommand(GetStoreId(), productId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{productId:guid}/publish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PublishProduct([FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        await _sender.Send(new PublishProductCommand(GetStoreId(), productId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{productId:guid}/unpublish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UnpublishProduct([FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        await _sender.Send(new UnpublishProductCommand(GetStoreId(), productId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{productId:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ArchiveProduct([FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ArchiveProductCommand(GetStoreId(), productId), cancellationToken);
        return NoContent();
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid!.Value;
}
