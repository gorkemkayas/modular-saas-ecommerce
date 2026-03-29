using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using Catalog.Application.Brands.Commands.ActivateBrand;
using Catalog.Application.Brands.Commands.CreateBrand;
using Catalog.Application.Brands.Commands.DeactivateBrand;
using Catalog.Application.Brands.Commands.UpdateBrand;
using Catalog.Application.Brands.DTOs;
using Catalog.Application.Brands.Queries.GetBrandById;
using Catalog.Application.Brands.Queries.SearchBrands;
using ECommerce.API.Contracts.Catalog.Brands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Catalog.Stores;

[Route("api/stores/me/brands")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class StoreBrandsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StoreBrandsController(ISender sender, ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<BrandDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchBrands([FromQuery] SearchBrandsRequest request, CancellationToken cancellationToken)
    {
        var brands = await _sender.Send(new SearchBrandsQuery(GetStoreId(), request.SearchTerm, request.ActiveOnly), cancellationToken);
        return Ok(brands);
    }

    [HttpGet("{brandId:guid}")]
    [ProducesResponseType(typeof(BrandDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBrandById([FromRoute] Guid brandId, CancellationToken cancellationToken)
    {
        var brand = await _sender.Send(new GetBrandByIdQuery(GetStoreId(), brandId), cancellationToken);
        return brand is null ? NotFound() : Ok(brand);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest request, CancellationToken cancellationToken)
    {
        var brandId = await _sender.Send(new CreateBrandCommand(GetStoreId(), request.Name, request.Slug, request.Description), cancellationToken);
        return CreatedAtAction(nameof(GetBrandById), new { brandId }, null);
    }

    [HttpPut("{brandId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateBrand(
        [FromRoute] Guid brandId,
        [FromBody] UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateBrandCommand(GetStoreId(), brandId, request.Name, request.Slug, request.Description), cancellationToken);
        return NoContent();
    }

    [HttpPost("{brandId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivateBrand([FromRoute] Guid brandId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateBrandCommand(GetStoreId(), brandId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{brandId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateBrand([FromRoute] Guid brandId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateBrandCommand(GetStoreId(), brandId), cancellationToken);
        return NoContent();
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid!.Value;
}
