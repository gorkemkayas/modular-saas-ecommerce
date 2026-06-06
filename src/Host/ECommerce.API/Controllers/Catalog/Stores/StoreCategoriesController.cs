using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using Catalog.Application.Categories.Commands.ActivateCategory;
using Catalog.Application.Categories.Commands.ChangeCategoryParent;
using Catalog.Application.Categories.Commands.CreateCategory;
using Catalog.Application.Categories.Commands.DeactivateCategory;
using Catalog.Application.Categories.Commands.UpdateCategory;
using Catalog.Application.Categories.DTOs;
using Catalog.Application.Categories.Queries.GetCategoryById;
using Catalog.Application.Categories.Queries.GetCategoryTree;
using ECommerce.API.Contracts.Catalog.Categories;
using ECommerce.API.Integrations.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Catalog.Stores;

[Route("api/stores/me/categories")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class StoreCategoriesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;
    private readonly IProductMediaStorageService _productMediaStorageService;

    public StoreCategoriesController(
        ISender sender,
        ITenantContext tenantContext,
        IProductMediaStorageService productMediaStorageService)
    {
        _sender = sender;
        _tenantContext = tenantContext;
        _productMediaStorageService = productMediaStorageService;
    }

    [HttpGet("tree")]
    [ProducesResponseType(typeof(IReadOnlyCollection<CategoryTreeNodeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryTree(CancellationToken cancellationToken)
    {
        var categories = await _sender.Send(new GetCategoryTreeQuery(GetStoreId()), cancellationToken);
        return Ok(categories);
    }

    [HttpGet("{categoryId:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById([FromRoute] Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _sender.Send(new GetCategoryByIdQuery(GetStoreId(), categoryId), cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var categoryId = await _sender.Send(new CreateCategoryCommand(
            GetStoreId(),
            request.Name,
            request.Slug,
            request.Description,
            request.ImageUrl,
            request.ParentCategoryId,
            request.SortOrder), cancellationToken);

        return CreatedAtAction(nameof(GetCategoryById), new { categoryId }, null);
    }

    [HttpPut("{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCategory(
        [FromRoute] Guid categoryId,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateCategoryCommand(
            GetStoreId(),
            categoryId,
            request.Name,
            request.Slug,
            request.Description,
            request.ImageUrl,
            request.SortOrder), cancellationToken);

        return NoContent();
    }

    [HttpPut("{categoryId:guid}/parent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeCategoryParent(
        [FromRoute] Guid categoryId,
        [FromBody] ChangeCategoryParentRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ChangeCategoryParentCommand(GetStoreId(), categoryId, request.ParentCategoryId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{categoryId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivateCategory([FromRoute] Guid categoryId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateCategoryCommand(GetStoreId(), categoryId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{categoryId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateCategory([FromRoute] Guid categoryId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateCategoryCommand(GetStoreId(), categoryId), cancellationToken);
        return NoContent();
    }

    [HttpPost("image/upload")]
    [ProducesResponseType(typeof(UploadCategoryImageFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UploadCategoryImageFile(
        [FromForm] UploadCategoryImageFileRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Category image is required.",
                Detail = "Select an image file before uploading."
            });
        }

        try
        {
            var uploadedFile = await _productMediaStorageService.UploadAsync(
                GetStoreId(),
                request.File,
                cancellationToken);

            if (!uploadedFile.IsImage)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Only image uploads are supported.",
                    Detail = "Category images must be uploaded as image files."
                });
            }

            return Ok(new UploadCategoryImageFileResponse(
                uploadedFile.Url,
                uploadedFile.OriginalFileName));
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Title = "Category image upload failed.",
                Detail = exception.Message
            });
        }
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid!.Value;
}
