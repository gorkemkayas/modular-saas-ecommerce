using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using Catalog.Application.Attributes.Commands.ActivateAttributeDefinition;
using Catalog.Application.Attributes.Commands.CreateAttributeDefinition;
using Catalog.Application.Attributes.Commands.DeactivateAttributeDefinition;
using Catalog.Application.Attributes.Commands.UpdateAttributeDefinition;
using Catalog.Application.Attributes.DTOs;
using Catalog.Application.Attributes.Queries.GetAttributeDefinitionById;
using Catalog.Application.Attributes.Queries.ListAttributeDefinitions;
using ECommerce.API.Contracts.Catalog.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Catalog.Stores;

[Route("api/stores/me/attributes")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
public sealed class StoreAttributesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StoreAttributesController(ISender sender, ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<AttributeDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAttributeDefinitions(
        [FromQuery] ListAttributeDefinitionsRequest request,
        CancellationToken cancellationToken)
    {
        var attributes = await _sender.Send(new ListAttributeDefinitionsQuery(GetStoreId(), request.ActiveOnly), cancellationToken);
        return Ok(attributes);
    }

    [HttpGet("{attributeDefinitionId:guid}")]
    [ProducesResponseType(typeof(AttributeDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttributeDefinitionById(
        [FromRoute] Guid attributeDefinitionId,
        CancellationToken cancellationToken)
    {
        var attributeDefinition = await _sender.Send(new GetAttributeDefinitionByIdQuery(GetStoreId(), attributeDefinitionId), cancellationToken);
        return attributeDefinition is null ? NotFound() : Ok(attributeDefinition);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAttributeDefinition(
        [FromBody] CreateAttributeDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var attributeDefinitionId = await _sender.Send(new CreateAttributeDefinitionCommand(
            GetStoreId(),
            request.Name,
            request.Code,
            request.DataType,
            request.IsRequired,
            request.IsFilterable,
            request.IsVariantDefining), cancellationToken);

        return CreatedAtAction(nameof(GetAttributeDefinitionById), new { attributeDefinitionId }, null);
    }

    [HttpPut("{attributeDefinitionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAttributeDefinition(
        [FromRoute] Guid attributeDefinitionId,
        [FromBody] UpdateAttributeDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateAttributeDefinitionCommand(
            GetStoreId(),
            attributeDefinitionId,
            request.Name,
            request.Code,
            request.DataType,
            request.IsRequired,
            request.IsFilterable,
            request.IsVariantDefining), cancellationToken);

        return NoContent();
    }

    [HttpPost("{attributeDefinitionId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ActivateAttributeDefinition(
        [FromRoute] Guid attributeDefinitionId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateAttributeDefinitionCommand(GetStoreId(), attributeDefinitionId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{attributeDefinitionId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateAttributeDefinition(
        [FromRoute] Guid attributeDefinitionId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateAttributeDefinitionCommand(GetStoreId(), attributeDefinitionId), cancellationToken);
        return NoContent();
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid!.Value;
}
