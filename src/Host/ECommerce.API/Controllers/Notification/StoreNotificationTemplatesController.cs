using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Notification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Notifications.Commands.ActivateNotificationTemplate;
using Notification.Application.Notifications.Commands.CreateNotificationTemplate;
using Notification.Application.Notifications.Commands.DeactivateNotificationTemplate;
using Notification.Application.Notifications.Commands.UpdateNotificationTemplate;
using Notification.Application.Notifications.DTOs;
using Notification.Application.Notifications.Queries.GetNotificationTemplateById;
using Notification.Application.Notifications.Queries.SearchNotificationTemplates;

namespace ECommerce.API.Controllers.Notification;

[Route("api/stores/me/notification-templates")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
[ApiExplorerSettings(GroupName = "v1")]
public sealed class StoreNotificationTemplatesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StoreNotificationTemplatesController(
        ISender sender,
        ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<NotificationTemplateSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] SearchNotificationTemplatesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SearchNotificationTemplatesQuery(
                GetStoreId(),
                request.Trigger,
                request.Channel,
                request.IsActive),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{templateId:guid}", Name = "GetNotificationTemplateById")]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid templateId,
        CancellationToken cancellationToken)
    {
        var template = await _sender.Send(
            new GetNotificationTemplateByIdQuery(GetStoreId(), templateId),
            cancellationToken);

        return template is null ? NotFound() : Ok(template);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateNotificationTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var templateId = await _sender.Send(
            new CreateNotificationTemplateCommand(
                GetStoreId(),
                request.Trigger,
                request.Channel,
                request.Locale,
                request.Name,
                request.SubjectTemplate,
                request.BodyTemplate),
            cancellationToken);

        return CreatedAtRoute("GetNotificationTemplateById", new { templateId }, new { TemplateId = templateId });
    }

    [HttpPut("{templateId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid templateId,
        [FromBody] UpdateNotificationTemplateRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateNotificationTemplateCommand(
                GetStoreId(),
                templateId,
                request.Locale,
                request.Name,
                request.SubjectTemplate,
                request.BodyTemplate),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{templateId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid templateId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateNotificationTemplateCommand(GetStoreId(), templateId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{templateId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid templateId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateNotificationTemplateCommand(GetStoreId(), templateId), cancellationToken);
        return NoContent();
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid ?? Guid.Empty;
}
