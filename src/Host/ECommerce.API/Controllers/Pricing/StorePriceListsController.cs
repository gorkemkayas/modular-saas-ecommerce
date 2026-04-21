using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Pricing.PriceLists;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pricing.Application.Common.Models;
using Pricing.Application.PriceLists.Commands.ActivatePriceEntry;
using Pricing.Application.PriceLists.Commands.ActivatePriceList;
using Pricing.Application.PriceLists.Commands.ArchivePriceList;
using Pricing.Application.PriceLists.Commands.ChangePriceListPriority;
using Pricing.Application.PriceLists.Commands.CreatePriceList;
using Pricing.Application.PriceLists.Commands.DeactivatePriceEntry;
using Pricing.Application.PriceLists.Commands.DeactivatePriceList;
using Pricing.Application.PriceLists.Commands.RemovePrice;
using Pricing.Application.PriceLists.Commands.RenamePriceList;
using Pricing.Application.PriceLists.Commands.SetDefaultPriceList;
using Pricing.Application.PriceLists.Commands.SetProductPrice;
using Pricing.Application.PriceLists.Commands.SetVariantPrice;
using Pricing.Application.PriceLists.DTOs;
using Pricing.Application.PriceLists.Queries.GetPriceListById;
using Pricing.Application.PriceLists.Queries.SearchPriceLists;
using Pricing.Application.Prices.DTOs;
using Pricing.Application.Prices.Queries.GetResolvedPrice;

namespace ECommerce.API.Controllers.Pricing;

[Route("api/stores/me/pricing/lists")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class StorePriceListsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StorePriceListsController(ISender sender, ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PriceListSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPriceLists(
        [FromQuery] SearchPriceListsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SearchPriceListsQuery(
            GetStoreId(),
            request.CurrencyCode,
            request.Status,
            request.PageNumber,
            request.PageSize), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{priceListId:guid}")]
    [ProducesResponseType(typeof(PriceListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPriceListById([FromRoute] Guid priceListId, CancellationToken cancellationToken)
    {
        var priceList = await _sender.Send(new GetPriceListByIdQuery(GetStoreId(), priceListId), cancellationToken);
        return priceList is null ? NotFound() : Ok(priceList);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePriceList(
        [FromBody] CreatePriceListRequest request,
        CancellationToken cancellationToken)
    {
        var priceListId = await _sender.Send(new CreatePriceListCommand(
            GetStoreId(),
            request.Name,
            request.CurrencyCode,
            request.Priority,
            request.IsDefault), cancellationToken);

        return CreatedAtAction(nameof(GetPriceListById), new { priceListId }, null);
    }

    [HttpPut("{priceListId:guid}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RenamePriceList(
        [FromRoute] Guid priceListId,
        [FromBody] RenamePriceListRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new RenamePriceListCommand(GetStoreId(), priceListId, request.Name), cancellationToken);
        return NoContent();
    }

    [HttpPut("{priceListId:guid}/priority")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePriceListPriority(
        [FromRoute] Guid priceListId,
        [FromBody] ChangePriceListPriorityRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ChangePriceListPriorityCommand(GetStoreId(), priceListId, request.Priority), cancellationToken);
        return NoContent();
    }

    [HttpPost("{priceListId:guid}/default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetDefaultPriceList([FromRoute] Guid priceListId, CancellationToken cancellationToken)
    {
        await _sender.Send(new SetDefaultPriceListCommand(GetStoreId(), priceListId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{priceListId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivatePriceList([FromRoute] Guid priceListId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivatePriceListCommand(GetStoreId(), priceListId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{priceListId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivatePriceList([FromRoute] Guid priceListId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivatePriceListCommand(GetStoreId(), priceListId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{priceListId:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ArchivePriceList([FromRoute] Guid priceListId, CancellationToken cancellationToken)
    {
        await _sender.Send(new ArchivePriceListCommand(GetStoreId(), priceListId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{priceListId:guid}/entries/{priceEntryId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivatePriceEntry(
        [FromRoute] Guid priceListId,
        [FromRoute] Guid priceEntryId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivatePriceEntryCommand(GetStoreId(), priceListId, priceEntryId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{priceListId:guid}/entries/{priceEntryId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivatePriceEntry(
        [FromRoute] Guid priceListId,
        [FromRoute] Guid priceEntryId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivatePriceEntryCommand(GetStoreId(), priceListId, priceEntryId), cancellationToken);
        return NoContent();
    }

    [HttpPut("{priceListId:guid}/products/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetProductPrice(
        [FromRoute] Guid priceListId,
        [FromRoute] Guid productId,
        [FromBody] SetProductPriceRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new SetProductPriceCommand(
            GetStoreId(),
            priceListId,
            productId,
            request.Amount,
            request.CompareAtAmount), cancellationToken);

        return NoContent();
    }

    [HttpDelete("{priceListId:guid}/products/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveProductPrice(
        [FromRoute] Guid priceListId,
        [FromRoute] Guid productId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new RemovePriceCommand(
            GetStoreId(),
            priceListId,
            productId,
            ProductVariantId: null), cancellationToken);

        return NoContent();
    }

    [HttpPut("{priceListId:guid}/products/{productId:guid}/variants/{productVariantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetVariantPrice(
        [FromRoute] Guid priceListId,
        [FromRoute] Guid productId,
        [FromRoute] Guid productVariantId,
        [FromBody] SetVariantPriceRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new SetVariantPriceCommand(
            GetStoreId(),
            priceListId,
            productId,
            productVariantId,
            request.Amount,
            request.CompareAtAmount), cancellationToken);

        return NoContent();
    }

    [HttpDelete("{priceListId:guid}/products/{productId:guid}/variants/{productVariantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveVariantPrice(
        [FromRoute] Guid priceListId,
        [FromRoute] Guid productId,
        [FromRoute] Guid productVariantId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new RemovePriceCommand(
            GetStoreId(),
            priceListId,
            productId,
            productVariantId), cancellationToken);

        return NoContent();
    }

    [HttpGet("resolve")]
    [ProducesResponseType(typeof(ResolvedPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResolvedPrice(
        [FromQuery] Guid productId,
        [FromQuery] Guid? productVariantId,
        [FromQuery] string currencyCode,
        CancellationToken cancellationToken)
    {
        var resolvedPrice = await _sender.Send(new GetResolvedPriceQuery(
            GetStoreId(),
            productId,
            productVariantId,
            currencyCode), cancellationToken);

        return resolvedPrice is null ? NotFound() : Ok(resolvedPrice);
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid ?? Guid.Empty;
}
