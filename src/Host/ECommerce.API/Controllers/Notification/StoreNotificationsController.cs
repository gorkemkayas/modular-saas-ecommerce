using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authorization;
using ECommerce.API.Contracts.Notification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Common.Models;
using Notification.Application.Notifications.DTOs;
using Notification.Application.Notifications.Queries.GetNotificationDispatchById;
using Notification.Application.Notifications.Queries.SearchNotificationDispatches;

namespace ECommerce.API.Controllers.Notification;

[Route("api/stores/me/notifications")]
[ApiController]
[Authorize(Policy = AppPolicies.TenantAdmin)]
[ApiExplorerSettings(GroupName = "v1")]
public sealed class StoreNotificationsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ITenantContext _tenantContext;

    public StoreNotificationsController(
        ISender sender,
        ITenantContext tenantContext)
    {
        _sender = sender;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDispatchSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] SearchNotificationDispatchesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SearchNotificationDispatchesQuery(
                GetStoreId(),
                request.Trigger,
                request.Channel,
                request.Status,
                request.BusinessEntityType,
                request.BusinessEntityId,
                request.PageNumber,
                request.PageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{dispatchId:guid}", Name = "GetNotificationDispatchById")]
    [ProducesResponseType(typeof(NotificationDispatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid dispatchId,
        CancellationToken cancellationToken)
    {
        var dispatch = await _sender.Send(
            new GetNotificationDispatchByIdQuery(GetStoreId(), dispatchId),
            cancellationToken);

        return dispatch is null ? NotFound() : Ok(dispatch);
    }

    private Guid GetStoreId() => _tenantContext.TenantIdAsGuid ?? Guid.Empty;
}
